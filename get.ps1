$client = New-Object System.Net.Sockets.TcpClient('localhost', 8000)
$stream = $client.GetStream()
$writer = New-Object System.IO.StreamWriter($stream)
$writer.AutoFlush = $true

# Create a StreamReader to receive data from the server
$reader = New-Object System.IO.StreamReader($stream)

# Send a command to the server (example: PING for Redis)
$pingcommand = "*1`r`n`$4`r`nPING`r`n"
$command = "*2`r`n$3`r`nGET`r`n$3`r`nfoo`r`n"
$writer.Write($command)  # Use Write instead of WriteLine

$response = $reader.ReadLine()

# Output the server's response
Write-Host "Server Response: $response"

# Clean up and close the connection
$reader.Close()
$writer.Close()
$client.Close()
