# services.py
import logging
import time
from datetime import datetime
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
import json
import os
from threading import Thread
from kivy.app import App


class Logger:
    COLORS = {
        "DEBUG": "\033[94m",  # Blue
        "INFO": "\033[92m",  # Green
        "WARNING": "\033[93m",  # Yellow
        "ERROR": "\033[91m",  # Red
        "CRITICAL": "\033[41m",  # Red background
        "RESET": "\033[0m"  # Reset to default color
    }

    def __init__(self, name):
        """Initialization of the Logger."""
        self._internal_logger = logging.getLogger(name)
        self._internal_logger.setLevel(logging.DEBUG)

        # Prevent propagation to the root logger to avoid double logging
        self._internal_logger.propagate = False

        # Remove all existing handlers associated with this logger to avoid conflicts
        if self._internal_logger.hasHandlers():
            self._internal_logger.handlers.clear()

        # Create handler
        handler = logging.StreamHandler()

        # Create formatter with color
        formatter = logging.Formatter(
            fmt='[%(asctime)s] [%(levelname)s] %(message)s',
            datefmt='%d.%m.%Y %H:%M:%S'
        )

        # Apply the formatter to the handler
        handler.setFormatter(self._colored_formatter(formatter))

        # Add the handler to the logger
        self._internal_logger.addHandler(handler)

    def _colored_formatter(self, formatter):
        """Return a formatter that adds color codes to the log message."""

        class ColoredFormatter(logging.Formatter):
            def format(self, record):
                color = Logger.COLORS.get(record.levelname, Logger.COLORS["RESET"])
                reset = Logger.COLORS["RESET"]
                formatted_message = super().format(record)
                return f"{color}{formatted_message}{reset}"

        return ColoredFormatter(fmt=formatter._fmt, datefmt=formatter.datefmt)

    def log(self, log_level, log_text):
        match log_level.lower():
            case "debug":
                self._internal_logger.debug(log_text)
            case "info":
                self._internal_logger.info(log_text)
            case "warning":
                self._internal_logger.warning(log_text)
            case "error":
                self._internal_logger.error(log_text)
            case "critical":
                self._internal_logger.critical(log_text)
            case _:
                self._internal_logger.info(f"Unknown log level: {log_level}. Message: {log_text}")


class ThreadManager:

    @property
    def logger(self):
        return self._logger

    def __init__(self, logger,max_workers=5):
        self._logger = logger
        self._executor = ThreadPoolExecutor(max_workers=max_workers)

    def submit_task(self, task, callback=None):
        def timed_task():
            task_started = time.time()
            result = task()
            task_completed = time.time()
            duration = task_completed - task_started
            return result, duration

        future = self._executor.submit(timed_task)

        if callback:
            def handle_task(internal_future):
                result, duration = internal_future.result()
                callback(task, result, duration)

            future.add_done_callback(handle_task)

    def on_task_complete(self, task, result, duration):
        task_name = task.__name__ if hasattr(task, '__name__') else str(task)
        self.logger.log("DEBUG", f"Task completed. Task: {task_name} - Result: {result} - Duration: {duration}")

    def start_daemon_thread(self, task):
        thread = Thread(target=task)
        thread.daemon = True
        thread.start()
        task_name = task.__name__ if hasattr(task, '__name__') else str(task)
        self.logger.log("DEBUG", f"Daemon thread started. Task: {task_name}")
