import os
import threading
from dotenv import load_dotenv


class Config:
    _instance = None
    _lock = threading.Lock()

    def __new__(cls):
        with cls._lock:
            if cls._instance is None:
                cls._instance = super().__new__(cls)
                load_dotenv()
                cls._instance._load_config()
        return cls._instance

    def _load_config(self):

        self.gemini_api_key = os.getenv("GEMINI_API_KEY")
        self.gemini_model_name = os.getenv("GEMINI_MODEL_NAME")

      #  self.openai_api_key = os.getenv("OPENAI_API_KEY")
       # self.openai_model_name = os.getenv("OPENAI_MODEL_NAME")

    def get(self, key, default=None):
        return getattr(self, key, default)


config = Config()
