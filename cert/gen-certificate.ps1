openssl req -new -x509 -days 36500 -newkey rsa:4096 -keyout privatekey.pem -out certificate.pem -nodes -subj "/CN=localhost" -sha256

openssl pkcs12 -export -out signing-certificate.pfx -inkey privatekey.pem -in certificate.pem