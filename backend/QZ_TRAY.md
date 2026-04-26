# QZ Tray certificate setup

QZ Tray uses a public certificate in the browser and a private key on the backend to trust print
requests without showing a confirmation alert each time.

Generate the files with OpenSSL:

```powershell
openssl req -x509 -newkey rsa:2048 -keyout private-key.pem -out digital-certificate.txt -days 3650 -nodes
```

Keep `private-key.pem` outside the frontend and outside public folders. Configure the API with
environment variables or user secrets:

```powershell
dotnet user-secrets set "QzTray:CertificatePath" "C:\certs\digital-certificate.txt" --project .\Renova.API\Renova.API.csproj
dotnet user-secrets set "QzTray:PrivateKeyPath" "C:\certs\private-key.pem" --project .\Renova.API\Renova.API.csproj
```

For containers or hosted environments, use `QzTray__Certificate`, `QzTray__PrivateKey`,
`QzTray__CertificatePath`, or `QzTray__PrivateKeyPath`. Multiline PEM values may use `\n`; the API
normalizes them before using the key.

Install or trust `digital-certificate.txt` in QZ Tray on the machines that will print. The frontend
loads `/api/qz/certificate` and signs QZ challenges through `/api/qz/sign`; the private key never
goes to the browser.
