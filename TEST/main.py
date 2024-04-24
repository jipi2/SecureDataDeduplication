from Crypto.PublicKey import RSA
from Crypto.Cipher import PKCS1_OAEP, AES
from Crypto.Random import get_random_bytes

class ProxyReEncryption:
    def __init__(self):
        # Generate keys for Alice, Bob, and the Proxy
        self.key_alice = RSA.generate(2048)
        self.key_bob = RSA.generate(2048)
        self.key_proxy = RSA.generate(2048)

    def encrypt(self, plaintext, public_key):
        # Encrypt plaintext using RSA encryption with OAEP padding
        cipher_rsa = PKCS1_OAEP.new(public_key)
        ciphertext = cipher_rsa.encrypt(plaintext.encode())
        return ciphertext

    def decrypt(self, ciphertext, private_key):
        # Decrypt ciphertext using RSA decryption with OAEP padding
        cipher_rsa = PKCS1_OAEP.new(private_key)
        plaintext = cipher_rsa.decrypt(ciphertext)
        return plaintext.decode()

    def generate_reencryption_key(self, from_key, to_key):
        # Generate session key (K)
        session_key = get_random_bytes(16)  # 16 bytes for AES-128

        # Encrypt the session key (K) with the public key of the 'to_key'
        cipher_rsa = PKCS1_OAEP.new(to_key)
        encrypted_session_key = cipher_rsa.encrypt(session_key)

        # Encrypt the private key of the 'from_key' using the session key (K) with AES
        cipher_aes = AES.new(session_key, AES.MODE_ECB)
        # Pad the private key to the block boundary using PKCS7 padding
        padded_private_key = self._pkcs7_pad(from_key.export_key())
        encrypted_private_key = cipher_aes.encrypt(padded_private_key)

        # Concatenate the encrypted session key and the encrypted private key
        reencryption_key = encrypted_session_key + encrypted_private_key

        return reencryption_key

    def _pkcs7_pad(self, data):
        # Calculate the number of bytes needed to pad
        padding_size = AES.block_size - len(data) % AES.block_size
        # Pad the data with bytes of value equal to the padding size
        padded_data = data + bytes([padding_size] * padding_size)
        return padded_data

    def reencrypt(self, ciphertext, reencryption_key):
        # Re-encrypt ciphertext using the re-encryption key
        cipher_rsa = PKCS1_OAEP.new(reencryption_key)
        reencrypted_text = cipher_rsa.encrypt(ciphertext)
        return reencrypted_text

# Example usage
proxy_reencryption = ProxyReEncryption()

# Encrypt data with Alice's public key
plaintext = "Hello, Bob! This message is from Alice."
ciphertext_alice = proxy_reencryption.encrypt(plaintext, proxy_reencryption.key_alice.publickey())

# Generate re-encryption key from Alice to Bob
reencryption_key = proxy_reencryption.generate_reencryption_key(proxy_reencryption.key_alice, proxy_reencryption.key_bob)

# Re-encrypt ciphertext for Bob using the re-encryption key
ciphertext_bob = proxy_reencryption.reencrypt(ciphertext_alice, reencryption_key)

# Decrypt ciphertext with Bob's private key
decrypted_text = proxy_reencryption.decrypt(ciphertext_bob, proxy_reencryption.key_bob)
print("Decrypted text by Bob:", decrypted_text)
