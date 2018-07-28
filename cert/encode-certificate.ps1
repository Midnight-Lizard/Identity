$fileContentBytes = get-content 'signing-certificate.txt' -Encoding Byte
[System.Convert]::ToBase64String($fileContentBytes) | Out-File 'signing-certificate+.txt'