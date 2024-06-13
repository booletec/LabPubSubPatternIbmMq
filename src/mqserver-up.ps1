 # Verifica se o Docker está instalado
$dockerInstalled = Get-Command docker -ErrorAction SilentlyContinue

if (-not $dockerInstalled) {
    Write-Output "Docker não está instalado. Instalando Docker..."
    
    # Baixar o instalador do Docker
    Invoke-WebRequest -Uri "https://download.docker.com/win/stable/Docker%20Desktop%20Installer.exe" -OutFile "$env:TEMP\DockerDesktopInstaller.exe"
    
    # Instalar o Docker
    Start-Process -Wait -FilePath "$env:TEMP\DockerDesktopInstaller.exe" -ArgumentList "/quiet"
    
    # Esperar o Docker ser instalado
    Start-Sleep -Seconds 20

    # Verifica novamente se o Docker está instalado
    $dockerInstalled = Get-Command docker -ErrorAction SilentlyContinue
    if ($dockerInstalled) {
        Write-Output "Docker foi instalado com sucesso."
    } else {
        Write-Output "Falha na instalação do Docker."
        exit 1
    }
} else {
    Write-Output "Docker já está instalado."
}

# Baixar e instalar a imagem Docker do IBM MQ
Write-Output "Baixando e instalando a imagem Docker do IBM MQ..."
 docker pull icr.io/ibm-messaging/mq:latest
Write-Output "Imagem Docker do IBM MQ foi baixada e instalada com sucesso."

Write-Output "Criando o volume qm1data"
docker volume create qm1data
Write-Output "Volume qm1data criado com sucesso"

Write-Output "rodando a imagem do servidor ibmmq na porta 1414"
docker run --env LICENSE=accept --env MQ_QMGR_NAME=QM1 --volume qm1data:/mnt/mqm --publish 1414:1414 --publish 9443:9443 --detach --env MQ_APP_PASSWORD=passw0rd --env MQ_ADMIN_PASSWORD=passw0rd --name QM1 icr.io/ibm-messaging/mq:latest
Write-Output "Servidor está rodando."

