$fileContentBytes = get-content 'signing-certificate.pfx' -Encoding Byte
[System.Convert]::ToBase64String($fileContentBytes) | Out-File 'signing-certificate.txt'