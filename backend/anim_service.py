# anim_service.py

from flask import Blueprint, jsonify
import random
import json
import time
import logging

from utils.gemini import GeminiModel
from events import EVENTS, events_bp
from states import STATES, states_bp

# Configure module‐level logger
debug_logger = logging.getLogger("anim_service")
debug_logger.setLevel(logging.DEBUG)

# store the last‐chosen animation so say_service can see it


# 2) Available animations
ANIMATIONS = [
    "A_POLY_IDL_ArmsFolded_Casual_Loop_Femn",
    "A_POLY_IDL_ArmsFolded_Grumpy_Loop_Femn",
    "A_POLY_IDL_Base_Femn",
    "A_POLY_IDL_Bored_FootTap_Femn",
    "A_POLY_IDL_Bored_KickGround_Femn",
    "A_POLY_IDL_Bored_SlumpBack_Femn",
    "A_POLY_IDL_Bored_SwingArms_Femn",
    "A_POLY_IDL_CheckWatch_Femn",
    "A_POLY_IDL_Drink_Shot_Femn",
    "A_POLY_IDL_Drink_Sip_Femn",
    "A_POLY_IDL_Drink_Swig_Femn",
    "A_POLY_IDL_Drink_Hold_Loop_Femn",
    "A_POLY_IDL_Eat_Large_Femn",
    "A_POLY_IDL_Eat_Scoff_Femn",
    "A_POLY_IDL_Eat_Small_Femn",
    "A_POLY_IDL_Eat_Hold_Loop_Femn",
    "A_POLY_IDL_HandsOnHips_Base_Loop_Femn",
    "A_POLY_IDL_HandsOnHips_Grumpy_Loop_Femn",
    "A_POLY_IDL_HeadNod_Greet_Femn",
    "A_POLY_IDL_HeadNod_Large_Femn",
    "A_POLY_IDL_HeadNod_Small_Femn",
    "A_POLY_IDL_HeadNod_Thanks_Femn",
    "A_POLY_IDL_HeadShake_Disappointed_Femn",
    "A_POLY_IDL_HeadShake_Large_Femn",
    "A_POLY_IDL_HeadShake_Small_Femn",
    "A_POLY_IDL_Inspect_Feet_Femn",
    "A_POLY_IDL_Inspect_Hands_Femn",
    "A_POLY_IDL_Inspect_Hips_Femn",
    "A_POLY_IDL_Inspect_Legs_Femn",
    "A_POLY_IDL_Inspect_Torso_Femn",
    "A_POLY_IDL_Look_Down_Femn",
    "A_POLY_IDL_Look_L_Behind_Femn",
    "A_POLY_IDL_Look_L_Femn",
    "A_POLY_IDL_Look_L_Scared_Femn",
    "A_POLY_IDL_Look_R_Behind_Femn",
    "A_POLY_IDL_Look_R_Femn",
    "A_POLY_IDL_Look_R_Scared_Femn",
    "A_POLY_IDL_Look_Up_Femn",
    "A_POLY_IDL_PickNose_Loop_Femn",
    "A_POLY_IDL_PlayWithHair_OneHand_TuckBehindEar_Femn",
    "A_POLY_IDL_PlayWithHair_TwoHands_FluffUp_Femn",
    "A_POLY_IDL_PlayWithHair_TwoHands_TuckBehindEars_Femn",
    "A_POLY_IDL_Plead_F_Femn",
    "A_POLY_IDL_Plead_Turning_Femn",
    "A_POLY_IDL_PointHand_Index_F_Femn",
    "A_POLY_IDL_PointHand_Index_F_Small_Femn",
    "A_POLY_IDL_PointHand_Index_L_Femn",
    "A_POLY_IDL_PointHand_Index_R_Femn",
    "A_POLY_IDL_PointHand_Thumb_L_Femn",
    "A_POLY_IDL_PointHand_Thumb_R_Femn",
    "A_POLY_IDL_Posture_Aggressive_Loop_Femn",
    "A_POLY_IDL_Posture_Dramatic_Loop_Femn",
    "A_POLY_IDL_Posture_Shy_Loop_Femn",
    "A_POLY_IDL_Posture_Slumped_Loop_Femn",
    "A_POLY_IDL_Pray_CrossChest_Femn",
    "A_POLY_IDL_Pray_Kneeling_Loop_Femn",
    "A_POLY_IDL_Pray_Standing_Loop_Femn",
    "A_POLY_IDL_Stretch_Arms_Femn",
    "A_POLY_IDL_Stretch_Calf_Femn",
    "A_POLY_IDL_Stretch_Legs_Femn",
    "A_POLY_IDL_Stretch_Quad_Femn",
    "A_POLY_IDL_Stretch_Shoulders_Femn",
    "A_POLY_IDL_Stretch_Squat_Femn",
    "A_POLY_IDL_Sway_Drunk_Loop_Femn",
    "A_POLY_IDL_Thoughtful_L_ChinScratch_Femn",
    "A_POLY_IDL_Thoughtful_R_ChinScratch_Femn",
    "A_POLY_IDL_Thoughtful_L_Loop_Femn",
    "A_POLY_IDL_Thoughtful_R_Loop_Femn",
    "A_POLY_IDL_UsePhone_OneHand_Photo_Femn",
    "A_POLY_IDL_UsePhone_OneHand_Scrolling_Femn",
    "A_POLY_IDL_UsePhone_OneHand_Typing_Femn",
    "A_POLY_IDL_UsePhone_TwoHands_Scrolling_Femn",
    "A_POLY_IDL_UsePhone_TwoHands_Typing_Femn",
    "A_POLY_IDL_UsePhone_OneHand_Loop_Femn",
    "A_POLY_IDL_UsePhone_TwoHands_Loop_Femn",
    "A_POLY_IDL_UsePhone_OneHand_Selfie_Loop_Femn",
    "A_POLY_IDL_UsePhone_TwoHands_Squinting_Loop_Femn",
    "A_POLY_IDL_UsePhone_OneHand_Video_Loop_Femn",
    "A_POLY_IDL_Wave_Double_Loop_Femn",
    "A_POLY_IDL_Wave_Large_Loop_Femn",
    "A_POLY_IDL_Wave_Small_Loop_Femn",
    "A_POLY_IDL_WeightShift_L_Femn",
    "A_POLY_IDL_WeightShift_R_Femn",
    "A_POLY_IDL_Yawn_Femn"
]

# Instantiate Gemini client
gemini = GeminiModel()
anim_bp = Blueprint("anim_service", __name__)

global LAST_ANIMATION

@anim_bp.route("/api/anim/witch-idle", methods=["GET"])
def choose_witch_idle_animation():
    global LAST_ANIMATION

    debug_logger.debug(f"EVENTS: {EVENTS}")
    debug_logger.debug(f"STATES: {STATES}")

    # High-temp ranking prompt
    high_prompt = f"""
You are an animation director. Here are the recent events:
{chr(10).join(f"- {e}" for e in EVENTS)}

Here are the current states:
{chr(10).join(f"- {s}" for s in STATES)}

Available animations:
{chr(10).join(f"- {a}" for a in ANIMATIONS)}

Your goal is to always choose the 3 most fitting animations from the list — even if the events and states are abstract. Never say 'none apply'.
Always give 3 animations and assign percentages summing to 100.
Respond in plain text like:
A=40%, B=30%, C=30%
"""
    debug_logger.debug("High-temp prompt to Gemini:\n%s", high_prompt)
    try:
        high_resp = gemini.call_model(
            high_prompt,
            system_prompt=None,
            check_malicious_input=True,
            image_paths=None,
            stream=False
        ).strip()
        debug_logger.debug("High-temp response: %s", high_resp)
    except Exception as e:
        debug_logger.exception("LLM ranking failed")
        return jsonify({"error": f"LLM ranking failed: {e}", "high_raw": None}), 500

    # Low-temp JSON formatting prompt
    low_prompt = f"""
Here is the raw ranking:
{high_resp}

Return STRICT JSON ONLY, keys matching animation names and integer values (no '%'): e.g.
{{
  "A_POLY_IDL_ArmsFolded_Casual_Loop_Femn": 40,
  "A_POLY_IDL_Yawn_Femn": 30,
  "A_POLY_IDL_Base_Femn": 30
}}
If you can't match exactly, respond ERROR.
"""
    debug_logger.debug("Low-temp prompt to Gemini:\n%s", low_prompt)

    parsed = None
    last_low_raw = None
    for attempt in range(3):
        try:
            low_resp = gemini.call_model(
                low_prompt,
                system_prompt=None,
                check_malicious_input=False
            ).strip()
            last_low_raw = low_resp
            # Strip any Markdown fences
            cleaned = "\n".join(
                line for line in low_resp.splitlines()
                if not line.strip().startswith("```")
            )
            debug_logger.debug("Low-temp cleaned (attempt %d): %s", attempt + 1, cleaned)

            # Parse JSON
            candidate = json.loads(cleaned)

            # Filter out any keys not in our ANIMATIONS list
            filtered = {k: v for k, v in candidate.items() if k in ANIMATIONS}
            debug_logger.debug("Filtered candidates: %s", filtered)

            # Validate that we still have entries
            if (
                    isinstance(filtered, dict)
                    and filtered
                    and all(isinstance(v, int) and v > 0 for v in filtered.values())
            ):
                parsed = filtered
                break
        except json.JSONDecodeError:
            parsed = None
        time.sleep(0.1)

    if not parsed:
        debug_logger.error("Failed to parse probabilities after 3 attempts.")
        return jsonify({
            "error": "Failed to parse animation probabilities",
            "high_raw": high_resp,
            "last_low_raw": last_low_raw
        }), 500

    # Weighted random pick
    names = list(parsed.keys())
    weights = [parsed[n] for n in names]
    chosen = random.choices(names, weights=weights, k=1)[0]
    LAST_ANIMATION = chosen

    debug_logger.debug(f"Chosen animation: {chosen} ; Candidates: {parsed}")
    debug_logger.info(f"Returning animation: {chosen}")

    return jsonify({"animation": chosen, "all_candidates": parsed}), 200
