import logging
import threading
import contextlib
import os
from datetime import datetime
from logging.handlers import TimedRotatingFileHandler


class LoggerManager:
    """manage the logger, set log level, and temporarily set log level"""
    _instance = None
    _lock = threading.Lock()

    def __new__(cls, name="logger", console_level=logging.INFO, file_level=logging.DEBUG, log_dir="log"):
        with cls._lock:
            if cls._instance is None:
                cls._instance = super().__new__(cls)
                cls._instance._init_logger(name, console_level, file_level, log_dir)
        return cls._instance

    def _init_logger(self, name, console_level, file_level, log_dir):
        self.logger = logging.getLogger(name)
        self.logger.setLevel(logging.DEBUG)
        self.logger.propagate = False

        # Create log directory if not exist
        os.makedirs(log_dir, exist_ok=True)

        log_file = os.path.join(log_dir, f"{datetime.now().strftime('%Y-%m-%d')}.log")

        if not self.logger.handlers:
            self.console_handler = logging.StreamHandler()

            self.file_handler = TimedRotatingFileHandler(
                log_file,
                when='midnight',
                interval=1,
                backupCount=14,  # Keep logs for 14 days
                encoding="utf-8"
            )

            self.console_handler.setLevel(console_level)
            self.file_handler.setLevel(file_level)

            formatter = logging.Formatter("%(asctime)s %(levelname)s:  %(message)s")
            self.console_handler.setFormatter(formatter)
            self.file_handler.setFormatter(formatter)

            self.logger.addHandler(self.console_handler)
            self.logger.addHandler(self.file_handler)
        else:
            self.console_handler = self.logger.handlers[0]
            self.file_handler = self.logger.handlers[1]

    def set_level(self, console_level=None, file_level=None):
        if console_level is not None:
            self.console_handler.setLevel(console_level)
        if file_level is not None:
            self.file_handler.setLevel(file_level)

    @contextlib.contextmanager
    def temp_level(self, console_level=None, file_level=None):
        """
        modify the logger level temporarily, and restore it after the context, e.g.:
        with logger_manager.temp_level(logging.DEBUG):
            logger.info("This will be shown even if console level was INFO")
        """
        old_console_level = self.console_handler.level
        old_file_level = self.file_handler.level

        self.set_level(console_level, file_level)
        try:
            yield
        finally:
            self.set_level(old_console_level, old_file_level)

    def get_logger(self):
        return self.logger


logger_manager = LoggerManager()  # the logger manager instance, to manage  log level, temporary log level, and logger
logger = logger_manager.get_logger()  # the logger instance to be used in the application
