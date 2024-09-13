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
    # Generate a random salt and IV
    salt = os.urandom(SALT_SIZE)
    iv = os.urandom(AES_BLOCK_SIZE)

    # Derive key using PBKDF2
    cipher_key = derive_key(key, salt)

    # Create AES cipher in CFB mode with the generated IV
    cipher = Cipher(algorithms.AES(cipher_key), modes.CFB(iv), backend=default_backend())
    encryptor = cipher.encryptor()

    # Read the original file data
    with open(file_path, 'rb') as file:
        data = file.read()

    # Encrypt the data
    encrypted_data = encryptor.update(data) + encryptor.finalize()

    # Save encrypted data with salt and iv at the beginning of the file
    with open(file_path + '.enc', 'wb') as file:
        file.write(salt + iv + encrypted_data)


def decrypt_file(file_path: str, key: bytes):
    """
    Decrypts a file using AES decryption.
    """
    with open(file_path, 'rb') as file:
        data = file.read()

    # Extract the salt, iv, and encrypted data
    salt = data[:SALT_SIZE]
    iv = data[SALT_SIZE:SALT_SIZE + AES_BLOCK_SIZE]
    encrypted_data = data[SALT_SIZE + AES_BLOCK_SIZE:]

    # Derive the key again using the same salt
    cipher_key = derive_key(key, salt)

    # Create AES cipher in CFB mode with the extracted IV
    cipher = Cipher(algorithms.AES(cipher_key), modes.CFB(iv), backend=default_backend())
    decryptor = cipher.decryptor()

    # Decrypt the data
    decrypted_data = decryptor.update(encrypted_data) + decryptor.finalize()

    # Save decrypted data
    with open(file_path.replace('.enc', ''), 'wb') as file:
        file.write(decrypted_data)



def encrypt_data_store(directory: str='', key: str=''):
    """
    Encrypts all files in the specified directory.
    """
    if directory == '':
        directory = DATA_DIR

    for filename in os.listdir(directory):
        if not filename.endswith('.enc'):
            encrypt_file(os.path.join(directory, filename), key.encode())


def decrypt_data_store(directory: str='', key: str=''):
    """
    Decrypts all files in the specified directory.
    """
    if directory == '':
        directory = DATA_DIR

    for filename in os.listdir(directory):
        if filename.endswith('.enc'):
            decrypt_file(os.path.join(directory, filename), key.encode())
