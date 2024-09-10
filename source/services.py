# services.py

class ServiceProvider:
    _instance = None
    _initialized = False

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(ServiceProvider, cls).__new__(cls)
        return cls._instance

    def __init__(self):
        """Initialization of the ServiceProvider (only executed once)."""
        if not self._initialized:
            self.services = {}
            print("ServiceProvider initialized")
            self._initialized = True

    def register_service(self, name, service):
        """Register a service in the ServiceProvider."""
        self.services[name] = service

    def get_service(self, name):
        """Retrieve a service by name."""
        return self.services.get(name)

