from umbral import SecretKey, Signer, PublicKey, encrypt, generate_kfrags, VerifiedCapsuleFrag, Capsule, decrypt_reencrypted, VerifiedKeyFrag, reencrypt
import base64

def generatePrivKey():
    user_priv_key = SecretKey.random()
    return base64.b64encode(user_priv_key.to_secret_bytes()).decode('utf-8')

def generatePubKey(base64key):
    user_priv_key = SecretKey.from_bytes(base64.b64decode(base64key))
    user_pub_key = user_priv_key.public_key()
    return base64.b64encode (user_pub_key.__bytes__()).decode('utf-8')

def encrypt_umbral(base64PrivKey, plaintext):
    privKey = SecretKey.from_bytes(base64.b64decode(base64PrivKey))
    pubKey = privKey.public_key()
    capsule, ciphertext = encrypt(pubKey, plaintext)
    base64Capsule = base64.b64encode(capsule.__bytes__()).decode('utf-8')
    base64Ciphertext = base64.b64encode(ciphertext).decode('utf-8')
    return base64Capsule+"#"+base64Ciphertext
    
def generateKfrag(base64PrivKey, base64PubKey):
    user_signing_key = SecretKey.random()
    user_signer = Signer(user_signing_key)
    privKey = SecretKey.from_bytes(base64.b64decode(base64PrivKey))
    pubKey = PublicKey.from_bytes(base64.b64decode(base64PubKey))
    kfrags = generate_kfrags(delegating_sk=privKey,
                         receiving_pk=pubKey,
                         signer=user_signer,
                         threshold=1,
                         shares=1)
    kfrag = kfrags[0]
    return base64.b64encode(kfrag.__bytes__()).decode('utf-8')

def decryptCapsule(base64PrivKey, base64PubKey, base64CFrag):
    privKey = SecretKey.from_bytes(base64.b64decode(base64PrivKey))
    pubKey = PublicKey.from_bytes(base64.b64decode(base64PubKey))
    
    split_str = base64CFrag.split("#")
    
    cfrag_bytes = base64.b64decode(split_str[0])
    capsule_bytes = base64.b64decode(split_str[1])
    ciphertext = base64.b64decode(split_str[2])
    
    cfrags = []
    cfrag = VerifiedCapsuleFrag.from_verified_bytes(cfrag_bytes.__bytes__())
    cfrags.append(cfrag)
    capsule = Capsule.from_bytes(capsule_bytes)
    plaintext = decrypt_reencrypted(receiving_sk=privKey,
                                        delegating_pk=pubKey,
                                        capsule=capsule,
                                        verified_cfrags=cfrags,
                                        ciphertext=ciphertext)
    
    return base64.b64encode(plaintext).decode('utf-8')

def __getCapsuleAndCipherTextFromBase64(base64cap):
    split_str = base64cap.split("#")
    base64Capsule = split_str[0]
    base64Ciphertext = split_str[1]
    capsule = Capsule.from_bytes(base64.b64decode(base64Capsule))
    ciphertext = base64.b64decode(base64Ciphertext)
    return capsule, ciphertext

base64Privkey1 = generatePrivKey()
privKey1 = SecretKey.from_bytes(base64.b64decode(base64Privkey1))
pubKey1 = privKey1.public_key()
base64PubKey1 = base64.b64encode(pubKey1.__bytes__()).decode('utf-8')

base64Privkey2 = generatePrivKey()
privKey2 = SecretKey.from_bytes(base64.b64decode(base64Privkey2))
pubKey2 = privKey2.public_key()
base64PubKey2 = base64.b64encode(pubKey2.__bytes__()).decode('utf-8')

base64kfrag = generateKfrag(base64Privkey1, base64PubKey2)
kFrag_bytes = base64.b64decode(base64kfrag)
kfrag = VerifiedKeyFrag.from_verified_bytes(kFrag_bytes)

plaintext = b'Hello World'

base64capsule_ciphertext = encrypt_umbral(base64Privkey1, plaintext)

capsule, ciphertext = __getCapsuleAndCipherTextFromBase64(base64capsule_ciphertext)
cfrag = reencrypt(capsule=capsule, kfrag=kfrag)

base64KeyCFrag = base64.b64encode(cfrag.__bytes__()).decode('utf-8')+"#"+ base64.b64encode(capsule.__bytes__()).decode('utf-8')+"#"+base64.b64encode(ciphertext).decode('utf-8')

plaintext = decryptCapsule(base64Privkey2, base64PubKey1, base64KeyCFrag)
print(base64.b64decode(plaintext))


# user_signing_key = SecretKey.random()
# pubKey = user_signing_key.public_key()
# base64PubKey = base64.b64encode(pubKey.__bytes__()).decode('utf-8')
# generateKfrag(base64key, base64PubKey)
# key = SecretKey.from_bytes(base64.b64decode(base64key))
# key = base64.b64decode(base64key)
# plaintext = b'Hello World'
# text = encrypt_umbral(base64key, plaintext)
# print(text)
# privKey = SecretKey.from_bytes(base64.b64decode(base64key))
# pubKey = privKey.public_key()
# capsule, ciphertext = encrypt(pubKey, plaintext)
# base64.b64encode(capsule.__bytes__()).decode('utf-8')
# print('asdad')