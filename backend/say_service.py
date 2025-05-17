from flask import Blueprint, jsonify
import random
import json
import time
import logging
import re

from utils.gemini import GeminiModel
from events import EVENTS
from states import STATES

logger = logging.getLogger("say_service")
logger.setLevel(logging.DEBUG)

say_bp = Blueprint("say_service", __name__)
gemini = GeminiModel()

@say_bp.route("/api/character-say", methods=["GET"])
def character_say():
    # Log context
    logger.debug("EVENTS: %s", EVENTS)
    logger.debug("STATES: %s", STATES)

    # 1) High-temp prompt
    high_prompt = f"""
You are a dialog writer for a WITCH game character.

Recent events:
{chr(10).join(f"- {e}" for e in EVENTS)}

Current scene states:
{chr(10).join(f"- {s}" for s in STATES)}

Suggest the top 3 short lines this character could say, each with a percentage weight that sums to 100.
Respond like:
"Hello there"=50%, "What a good kitty"=30%, "My cat is nice cat, go to mommy"=20%
ACT AS WITCH CHARACTER IN GAME! not a cat! WITCH PERSON CHARACTER !
"""
    try:
        high_resp = gemini.call_model(
            high_prompt,
            system_prompt=None,
            check_malicious_input=False,
            image_paths=None,
            stream=False
        ).strip()
        logger.debug("High-temp response: %s", high_resp)
    except Exception as e:
        logger.exception("LLM generation failed")
        return jsonify({"error": f"LLM generation failed: {e}", "high_raw": None}), 500

    # 2) Low-temp JSON formatting prompt
    low_prompt = f"""
Here is the raw ranking:
{high_resp}

Convert this into STRICT JSON ONLY, where keys are the full lines (strings) and values are integers (no '%' sign):
{{
  "Hello there": 50,
  "Good kitti": 30,
  "What a pretty kitti, go to mommy": 20
}}
If you cannot match exactly, respond with ERROR.
"""
    parsed = None
    last_low = None

    for attempt in range(3):
        try:
            low_resp = gemini.call_model(
                low_prompt,
                system_prompt=None,
                check_malicious_input=False,
                stream=False
            ).strip()
            last_low = low_resp
            # strip markdown fences
            cleaned = "\n".join(
                line for line in low_resp.splitlines()
                if not line.strip().startswith("```")
            ).strip()
            logger.debug("Low-temp cleaned (attempt %d): %s", attempt+1, cleaned)
            candidate = json.loads(cleaned)
            # validate keys and values
            if (
                isinstance(candidate, dict)
                and all(isinstance(k, str) for k in candidate.keys())
                and all(isinstance(v, int) and v > 0 for v in candidate.values())
                and sum(candidate.values()) == 100
            ):
                parsed = candidate
                break
        except json.JSONDecodeError:
            parsed = None
        time.sleep(0.1)

    # fallback: parse directly from high_resp if low failed
    if not parsed:
        logger.warning("Low-temp JSON failed, falling back to regex parse from high_resp")
        parsed = {}
        for m in re.finditer(r'"([^"]+)"\s*=\s*(\d+)%', high_resp):
            line, pct = m.group(1), int(m.group(2))
            parsed[line] = pct
        if not parsed:
            logger.error("Regex fallback also failed")
            return jsonify({
                "error": "Failed to parse character-say probabilities",
                "high_raw": high_resp,
                "last_low_raw": last_low
            }), 500

    # 3) Weighted random selection
    lines = list(parsed.keys())
    weights = [parsed[l] for l in lines]
    chosen = random.choices(lines, weights=weights, k=1)[0]
    logger.info("Chosen line: %s", chosen)

    return jsonify({
        "chosen": chosen,
        "candidates": parsed
    }), 200
