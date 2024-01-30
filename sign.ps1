Import-Module PKI
Set-Variable storeLocation -Option Constant -Value "cert:\LocalMachine\My"
Set-Variable domain -Option Constant -Value "localhost"
Set-Variable ipv4 -Option Constant -Value "127.0.0.1"
Set-Variable ipv6 -Option Constant -Value "::1"
Set-Variable country -Option Constant -Value "EG"
Set-Variable author -Option Constant -Value "asm2025"
Set-Variable email -Option Constant -Value "asm2025@outlook.com"
Set-Variable exportedFileName -Option Constant -Value "sign.pfx"

try
{
	$notBefore = [datetime]::Today;
	$notAfter = $notBefore.AddYears(50);
	$certParams = @{
		Subject = "CN=$domain,O=$author,C=$country"
		FriendlyName = "$author self-signed programming certificate"
		TextExtension = @(
			"2.5.29.17={text}DNS=$domain&IPAddress=$ipv4&IPAddress=$ipv6",
			"2.5.29.19={text}",
			"2.5.29.37={text}1.3.6.1.5.5.7.3.3"
		)
		KeyUsage = @("CertSign", "CRLSign", "DigitalSignature")
		KeyUsageProperty = "All"
		KeyExportPolicy = "Exportable"
		CertStoreLocation = $storeLocation
		NotBefore = $notBefore
		NotAfter = $notAfter
	};
	$thumb = (New-SelfSignedCertificate @certParams).Thumbprint;
	Write-Host "`nThe new certificate was created at the '$storeLocation' store successfuly.`nNew certificate's thumbprint is '$thumb'.`n";
	
	$importConfirmation = Read-Host "Would you like to add the new certificate to the trusted roots [y]";
	
	if ($importConfirmation -eq "y")
	{
		$cert = Get-Item "$storeLocation\$thumb";
		$location = [Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine;
		$storeName = [Security.Cryptography.X509Certificates.StoreName]::Root;
		$store = New-Object System.Security.Cryptography.X509Certificates.X509Store -ArgumentList $storeName, $location
		$openFlags = [System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite
		$store.Open($openFlags)
		$store.Add($cert);
		$store.Close();
		Write-Host "`nThe certificate was added to the trusted root store successfuly.`n";
	}
	
	$exportConfirmation = Read-Host "Would you like to export the new certificate [y]";
	
	if ($exportConfirmation -eq "y")
	{
		$pwd = Read-Host "Type in the certificate's password" -AsSecureString;
		$exportParams = @{
			cert = "$storeLocation\$thumb"
			FilePath = $exportedFileName
		};
		
		if ($pwd)
		{
			$exportParams.Add("Password", $pwd);
		}
		
		Export-PfxCertificate @exportParams;
		Write-Host "`nThe certificate was exported successfully.`n"
	}
	
	Write-Host @"
To check the certificate follow these steps:
1. Run mmc.exe
2. From the file menu, and click the menu Add/Remove Snap-in...
3. From the snap-in column, Add Certificates for the 'Computer account' and click Next
4. In the next screen, keep the default 'Local computer' option selected and click Finish
5. You should be able to find the new certificate under the node:
   'Certificates (Local Computer)\Personal\Certificates'
   that is issued to '$author'

*  Hit refresh from the right-click if needed
"@
	
	if ($exportConfirmation -ne 'y')
	{
		Write-Host "*  You can export the certificate with the thumbrint '$thumb' to a pfx or cert file."
	}
}
catch
{
	Write-Error -ErrorRecord $_;
	$errorCode = $_.Exception.HResult;
	
	if ($errorCode -eq 0)
	{
		$errorCode = 1;
	}
	
	exit $errorCode;
}
