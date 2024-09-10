import gi
from gi.repository import Gtk

def test_gtk():
    Gtk.init(None)
    print("GTK is working")

if __name__ == "__main__":
    test_gtk()
