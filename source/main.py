# main.py

from gui.application import Application
from source.data import Database, DatabaseService

if __name__ == '__main__':

    db = Database('VirtualServer', 'AdventureWorks2022')
    db_service = DatabaseService()
    db_service.databases['AdventureWorks2022'] = db
    db_service.databases['AdventureWorks2022'].import_tables_from_csv_files()

    app = Application()
    app.run()