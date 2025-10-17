# KeyPairGenerator (Example)

For anybody that wants to start making their Key Generator for a specific app. (not GUI)


(compile with csc)
__How to use?:__
  - KeyPairGenerator.cs — run once to create private_key.xml and public_key.xml.
  - LicenseGenerator.cs — run when you want to create a token (uses private key).
          - Example: __LicenseGenerator.exe private_key.xml owner 60__
  - TokenVerifier.cs — use inside your custom app to verify tokens (uses public key).
  - LoginForm.cs — use this to make your login menu before the entry of your main app. 
