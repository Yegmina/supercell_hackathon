# anim_service.py

from flask import Blueprint, jsonify
import random
import json
import time
import logging

from utils.gemini import GeminiModel

# Configure module-level logger
debug_logger = logging.getLogger("anim_service")
debug_logger.setLevel(logging.DEBUG)

# 1) Hard-coded list of recent events (for prototyping)
_EVENTS = [
    "cat stands and moves near X",
    "cat looks at X",
    "cat passed X",
    "cat made a sound X",
    "cat sat down",
    "cat lay down"
]

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

# Utility to strip Markdown code fences

def strip_code_fences(text):
    """
    Remove lines starting and ending with triple backticks from the LLM response.
    """
    lines = text.splitlines()
    cleaned = [line for line in lines if not line.strip().startswith("```")]
    return "\n".join(cleaned)

# 3) Instantiate your Gemini client once
gemini = GeminiModel()

# 4) Create a Blueprint to hold our route
anim_bp = Blueprint("anim_service", __name__)


@anim_bp.route("/api/anim/witch-idle", methods=["GET"])
def choose_witch_idle_animation():
    """
    1) Sends the hard-coded events to Gemini at high temperature to get a ranking
    2) Sends that raw ranking back to Gemini at low temperature to get strict JSON
    3) Parses & validates the names, retries if necessary
    4) Randomly selects one by weight and returns it
    """
    # --- 1) high-temp ranking prompt
    high_prompt = f"""
You are an animation director. Here are the recent events:
{chr(10).join(f"- {e}" for e in _EVENTS)}

Available animations:
{chr(10).join(f"- {a}" for a in ANIMATIONS)}

Your goal is to always choose the 3 most fitting animations from the list below — even if the events are abstract, metaphorical, or unrelated. Choose creatively if needed, but never say "none apply". Always give 3 animations and assign percentages (summing to 100).
Respond in plain text exactly like:
A=40%, B=30%, C=30%



to test 
curl http://127.0.0.1:5000/api/anim/witch-idle -o resp.json
notepad resp.json

"""

    debug_logger.debug("Sending high-temp ranking prompt to Gemini:")
    debug_logger.debug(high_prompt)
    try:
        high_resp = gemini.call_model(
            high_prompt,
            system_prompt=None,
            check_malicious_input=True,
            image_paths=None,
            stream=False
        ).strip()
        debug_logger.debug(f"High-temp response: {high_resp}")
    except Exception as e:
        debug_logger.exception("LLM ranking call failed")
        return jsonify({"error": f"LLM ranking failed: {e}", "high_raw": None}), 500

    # --- 2) low-temp JSON formatting + verify loop
    low_prompt = f"""
Here is a raw ranking from the director:
{high_resp}

Return STRICT JSON ONLY, where keys match exactly the animation names and values are integers (no '%' sign). For example:
{{
  \"A_POLY_IDL_ArmsFolded_Casual_Loop_Fem\": 40,
  \"A_POLY_IDL_LookAround_Casual_Loop_Fem\": 30,
  \"A_POLY_IDL_Yawn_Casual_Loop_Fem\": 30
}}
If you cannot match exactly, respond with ERROR.
"""
    debug_logger.debug("Sending low-temp JSON-formatting prompt to Gemini:")
    debug_logger.debug(low_prompt)

    parsed = None
    last_low_resp = None
    for attempt in range(1, 4):
        try:
            low_resp = gemini.call_model(
                low_prompt,
                system_prompt=None,
                check_malicious_input=False
            ).strip()
            last_low_resp = low_resp
            debug_logger.debug(f"Attempt {attempt} raw low-temp response: {low_resp}")

            # Strip Markdown fences if present
            cleaned = strip_code_fences(low_resp)
            debug_logger.debug(f"Attempt {attempt} cleaned response: {cleaned}")

            parsed = json.loads(cleaned)
            # Validate keys & values
            if (
                isinstance(parsed, dict)
                and set(parsed.keys()).issubset(set(ANIMATIONS))
                and all(isinstance(v, int) and v > 0 for v in parsed.values())
            ):
                debug_logger.debug("Parsed and validated JSON successfully.")
                break
            else:
                debug_logger.error(f"Validation failed on attempt {attempt}: {parsed}")
                parsed = None
        except json.JSONDecodeError:
            debug_logger.error(f"JSON decode error on attempt {attempt}: {last_low_resp}")
            parsed = None
        except Exception as e:
            debug_logger.exception(f"Error during low-temp attempt {attempt}")
            parsed = None
        time.sleep(0.1)

    if not parsed:
        debug_logger.error("Failed to parse/validate animation probabilities after 3 attempts.")
        return jsonify({
            "error": "Failed to parse animation probabilities",
            "high_raw": high_resp,
            "last_low_raw": last_low_resp
        }), 500

    # --- 3) weighted random pick
    names = list(parsed.keys())
    weights = [parsed[n] for n in names]
    chosen = random.choices(names, weights=weights, k=1)[0]
    debug_logger.debug(f"Chosen animation: {chosen} (weights: {parsed})")

    return jsonify({"animation": chosen, "all_candidates": parsed})
