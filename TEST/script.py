from umbral import SecretKey, Signer, encrypt, decrypt_original, generate_kfrags, reencrypt, decrypt_reencrypted, PublicKey, VerifiedKeyFrag
import json

alice_priv_key = SecretKey.random()
alice_pub_key = alice_priv_key.public_key()

alice_signing_key = SecretKey.random()
alice_signer = Signer(alice_signing_key)
alice_verifying_key = alice_signing_key.public_key()

bob_priv_key = SecretKey.random()
bob_pub_key = bob_priv_key.public_key()

kfrags = generate_kfrags(delegating_sk=alice_priv_key,
                         receiving_pk=bob_pub_key,
                         signer=alice_signer,
                         threshold=1,
                         shares=1)
print(type(kfrags))

kfragment = kfrags[0]
bytes_kfragment = kfragment.__bytes__()

new_kfragment = VerifiedKeyFrag.from_verified_bytes(bytes_kfragment)

plaintext = b'Proxy Re-encryption is cool!'
capsule, ciphertext = encrypt(alice_pub_key, plaintext)

cfrag = reencrypt(capsule=capsule,kfrag=new_kfragment)

print(cfrag)
cfrags = list()
cfrags.append(cfrag)

bob_cleartext = decrypt_reencrypted(receiving_sk=bob_priv_key,
                                        delegating_pk=alice_pub_key,
                                        capsule=capsule,
                                        verified_cfrags=cfrags,
                                        ciphertext=ciphertext)

print(str(bob_cleartext, 'utf-8'))