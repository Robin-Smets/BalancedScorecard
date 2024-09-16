# services.py
import time
from datetime import datetime
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
import json
import os
from threading import Thread
from kivy.app import App

class Logger:

    def __init__(self):
        """Initialization of the ServiceProvider."""
        self._log = {}

    def log(self, log_level, message):
        timestamp = datetime.now().strftime("%m/%d/%Y %H:%M:%S")
        log_text = f"{timestamp} [{log_level.name}]: {message}"

class ThreadManager:

    @property
    def logger(self):
        return App.get_running_app().logger

    def __init__(self, max_workers=5):
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
