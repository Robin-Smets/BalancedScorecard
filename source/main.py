# main.py

from gui.application import Application
from source.data import Database, DatabaseService

if __name__ == '__main__':

    db = Database('WindowsVM', 'DataWarehouse', 'SQL+Server')

    db_service = DatabaseService()
    db_service.databases['DataWarehouse'] = db

    app = Application()
    app.run()