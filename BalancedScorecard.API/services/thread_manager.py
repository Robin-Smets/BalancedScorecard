# thread_manager.py

from datetime import datetime
from threading import Thread


class ThreadManager:

    @property
    def daemon_thread_log(self):
        return self._daemon_thread_log

    def __init__(self):
        self._daemon_thread_log = dict()


    def start_daemon_thread(self, task, *args):
        # define wrapper
        def task_wrapper():
            task(*args)
            self.daemon_thread_log[datetime.now()] = f"Task '{task_name}' has completed."
        # start task with logging wrapper
        task_name = task.__name__ if hasattr(task, '__name__') else str(task)
        thread = Thread(target=task_wrapper)
        thread.daemon = True
        thread.start()
        self.daemon_thread_log[datetime.now()] = f"Daemon thread started. Task: {task_name}"

