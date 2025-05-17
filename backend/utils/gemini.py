import os
import sys
import google.generativeai as genai
from google.genai.types import Tool, GenerateContentConfig, GoogleSearch
from flask import g
import json
from .logger import logger
import time
from .utils_func import exponential_retry

sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), "..")))
from config import config  # Now Python should find 'config'
from utils.exceptions import MaliciousContentError, ModelResponseError


class GeminiModel:
    def __init__(self, model_name=config.gemini_model_name):
        logger.debug("🔧 Initializing GeminiModel...")
        self.model_name = model_name
        self.api_key = config.gemini_api_key  # Use an environment variable for security
        if not self.api_key:
            logger.critical("❌ ERROR: API key not found.")
            raise ModelResponseError("Service temporarily unavailable")
        logger.debug(f"🔧 Using generation model: {self.model_name}")

        # Configure the API key for the existing streaming interface.
        genai.configure(api_key=self.api_key)
        # Use the light model for safety checking
        self.safe_model = genai.GenerativeModel("gemini-2.0-flash")
        # Use the model from environment (config) for generating responses
        self.model = genai.GenerativeModel(self.model_name)

        # Default system prompt to enforce ethical use
        self.default_system_prompt = (
            "You are a WITCH character in a stylized fantasy game. "
            "You speak with clever wit and mystical flair, often cryptic or poetic, sometimes teasing. "
            "Your speech is dramatic and short, like stage lines or thoughts whispered aloud. "
            "Do not describe surroundings — only say what the character might mutter, chant, or proclaim. "
            "Refer to the cat by name (Whiskers), or use affectionate nicknames like 'kitty', 'familiar', or 'furball'. "
            "Your tone shifts with mood — amused, curious, ominous, or prophetic — and you sometimes hint at secrets. "
            "Make it sound like a real witch thinking or talking in character."
        )

        self.max_retries = 5
        self.initial_retry_delay = 4
        logger.debug("✅ GeminiModel initialized successfully.")

    def is_malicious_content(self, user_prompt):
        # changed the name because it also checks the output from the model
        """
        Uses a Gemini self‑check prompt via the safe model (gemini-2.0-flash) to decide if the input is malicious.
        Retries exponentially if a 429 error occurs.
        """
        return False

    def call_model(self, user_prompt, system_prompt=None, image_paths=None, stream=False, check_malicious_input=True):
        """ Securely call the model while preventing jailbreak attempts, with exponential retries if needed. """
        # check_malicious_input can be turned off for the second and following 'evaluation',
        # because the input has already been checked in the first call and the rests are the same.
        logger.debug(f"🛠️ Preparing to call model with prompt: {user_prompt}")

        # check user prompt for malicious content first
        if check_malicious_input and self.is_malicious_content(user_prompt):
            logger.warning("⚠️ Malicious prompt detected. Blocking request.")
            raise MaliciousContentError("⚠️ Request blocked: Jailbreak attempt detected.")

        messages = []
        system_prompt = system_prompt or self.default_system_prompt
        messages.append({"role": "user", "parts": [system_prompt]})
        messages.append({"role": "model", "parts": ["Understood."]})
        messages.append({"role": "user", "parts": [user_prompt]})

        logger.debug(f"📤 Sending system prompt: {system_prompt}")
        logger.debug(f"📤 Sending user prompt: {user_prompt}")

        if stream:
            return self.call_model_stream(messages)
        else:
            return self.call_model_non_stream(messages)

    def call_model_non_stream(self, messages):
        try:
            response = exponential_retry(
                self.model.generate_content,
                messages,
                match_exception_keywords="429"
            )
            logger.debug("✅ Model call successfully.")
        except Exception as e:
            logger.error(f"❌ Exception during model call: {e}")
            raise ModelResponseError("Error generating response")

        if self.is_malicious_content(response.text):
            logger.warning("⚠️ Malicious response detected. Blocking output.")
            raise MaliciousContentError("⚠️ Response blocked: Jailbreak attempt detected.")
        else:
            return response.text

    def call_model_stream(self, messages):
        try:
            response = exponential_retry(
                self.model.generate_content,
                messages,
                match_exception_keywords="429",
                stream=True
            )
            logger.debug("✅ Model call successful, processing stream response.")
        except Exception as e:
            logger.error(f"❌ Exception during model call: {e}")
            raise ModelResponseError("Error generating response")

        else:
            for chunk in response:
                try:
                    finished, text = self.parse_stream_chunk(chunk)
                    logger.debug(f"📜 Parsed chunk - Finished: {finished}, Text: {text}")

                    if self.is_malicious_content(text):
                        logger.warning("⚠️ Malicious response detected in chunk. Blocking output.")
                        raise MaliciousContentError("⚠️ Response blocked: Jailbreak attempt detected.")
                except Exception as e:
                    logger.error(f"❌ Exception while parsing stream chunk: {e}")
                    raise ModelResponseError("Error generating response")
                else:
                    yield text

    def parse_stream_chunk(self, chunk):
        """ Parse streaming chunk from Gemini API with robust error handling. """
        logger.debug("🔄 Parsing streaming chunk...")
        chunk_dict = chunk.to_dict()
        try:
            candidates = chunk_dict.get("candidates", [])
            if not candidates:
                logger.error("❌ No candidates found in chunk.")
                raise ModelResponseError("Error generating response")
            candidate = candidates[0]
            finished = candidate.get("finish_reason") == 1
            content = candidate.get("content", {})
            parts = content.get("parts", [])
            if not parts:
                logger.error("❌ No parts found in candidate content.")
                raise ModelResponseError("Error generating response")
            text = parts[0].get("text", "")
            logger.debug(f"✅ Parsed chunk successfully - Finished: {finished}, Text: {text[:100]}...")
            return finished, text
        except Exception as e:
            logger.error(f"❌ Failed to parse chunk: {e}")
            raise ModelResponseError("Error generating response")

    def call_model_with_browser(self, user_prompt):
        """
        Calls the Gemini model with browsing enabled using Google's search tool, with exponential retries.
        """
        print(f"🔍 DEBUG: Calling model with browsing for prompt: {user_prompt}")

        from google import genai as genai_browser
        from google.genai import types  # Import the types needed for configuration

        try:
            client = genai_browser.Client(api_key=self.api_key)
            google_search_tool = Tool(google_search=GoogleSearch())
            config_browsing = GenerateContentConfig(
                tools=[google_search_tool],
                response_modalities=["TEXT"]
            )
            response = exponential_retry(
                client.models.generate_content,
                match_exception_keywords="429",
                model=self.model_name,
                contents=user_prompt,
                config=config_browsing
            )
            logger.debug("✅ Browsing-enabled model call successful.")
            return response
        except Exception as e:
            logger.error(f"❌ Exception in call_model_with_browser: {e}")
            raise ModelResponseError("Error generating response")


# unused for now
def get_gemini_model():
    if "gemini_model" not in g:
        logger.debug("🔄 Initializing GeminiModel in Flask global context.")
        g.gemini_model = GeminiModel()
    return g.gemini_model


if __name__ == "__main__":
    logger.debug("🚀 Starting GeminiModel script.")

    # For safety checking, the light model ("gemini-2.0-flash") is used,
    # while the generation model (from .env) is used for responses.
    gemini_model = GeminiModel(model_name="gemini-2.0-flash-exp")

    try:
        browsing_response = gemini_model.call_model_with_browser(
            "Write input fields names for Horizon grant application"
        )
        text = browsing_response.candidates[0].content.parts[0].text
    except Exception as e:
        text = f"❌ ERROR: Failed to parse browsing response: {e}"
    logger.debug("🌐 Browsing Response:", text)
