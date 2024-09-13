from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC
from cryptography.hazmat.primitives.kdf.scrypt import Scrypt
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.kdf.scrypt import Scrypt
from cryptography.hazmat.primitives import hashes
import os
import base64
import getpass
import pandas as pd
from pathlib import Path

# Parameters for AES encryption
AES_KEY_SIZE = 32  # 256-bit key
AES_BLOCK_SIZE = 16
SALT_SIZE = 16
ITERATIONS = 100000

app_path = os.getcwd()
parent_dir = os.path.dirname(app_path)
DATA_DIR = f'{parent_dir}/data'

def derive_key(password: bytes, salt: bytes) -> bytes:
    """
    Derives a key from a password and salt using PBKDF2.
    """
    kdf = PBKDF2HMAC(
        algorithm=hashes.SHA256(),
        length=AES_KEY_SIZE,
        salt=salt,
        iterations=ITERATIONS,
        backend=default_backend()
    )
    return kdf.derive(password)


def encrypt_file(file_path: str, key: bytes):
    """
    Encrypts a file using AES encryption.
    """
    salt = os.urandom(SALT_SIZE)
    cipher = Cipher(algorithms.AES(derive_key(key, salt)), modes.CFB(os.urandom(AES_BLOCK_SIZE)),
                    backend=default_backend())
    encryptor = cipher.encryptor()

    with open(file_path, 'rb') as file:
        data = file.read()

    encrypted_data = encryptor.update(data) + encryptor.finalize()

    # Save encrypted data with salt and iv
    with open(file_path + '.enc', 'wb') as file:
        file.write(salt + cipher.algorithm.key + cipher.mode.iv + encrypted_data)


def decrypt_file(file_path: str, key: bytes):
    """
    Decrypts a file using AES decryption.
    """
    with open(file_path, 'rb') as file:
        data = file.read()

    salt = data[:SALT_SIZE]
    key = derive_key(key, salt)
    iv = data[SALT_SIZE + AES_KEY_SIZE:SALT_SIZE + AES_KEY_SIZE + AES_BLOCK_SIZE]
    encrypted_data = data[SALT_SIZE + AES_KEY_SIZE + AES_BLOCK_SIZE:]

    cipher = Cipher(algorithms.AES(key), modes.CFB(iv), backend=default_backend())
    decryptor = cipher.decryptor()

    decrypted_data = decryptor.update(encrypted_data) + decryptor.finalize()

    # Save decrypted data
    with open(file_path.replace('.enc', ''), 'wb') as file:
        file.write(decrypted_data)


def encrypt_data_store(directory: str=''):
    """
    Encrypts all files in the specified directory.
    """
    if directory == '':
        directory = DATA_DIR
    key = getpass.getpass(prompt="Enter encryption key: ").encode()
    for filename in os.listdir(directory):
        if not filename.endswith('.enc'):
            encrypt_file(os.path.join(directory, filename), key)


def decrypt_data_store(directory: str=''):
    """
    Decrypts all files in the specified directory.
    """
    if directory == '':
        directory = DATA_DIR
    key = getpass.getpass(prompt="Enter decryption key: ").encode()
    for filename in os.listdir(directory):
        if filename.endswith('.enc'):
            decrypt_file(os.path.join(directory, filename), key)
