# Set the path to the docker-compose file to ensure the script can be run from any directory
$composeFile = ".\docker-compose.yml"

# Stop and remove containers defined in the compose file
Write-Host "Stopping and removing containers via docker-compose..."
docker-compose -f $composeFile down --volumes --remove-orphans

# --- New Robust Cleanup Logic ---
Write-Host "Searching for any containers using 'consilient' images..."

# 1. Find all images with "consilient" in their name.
$imageIds = docker images --filter "reference=*consilient*" -q

if ($imageIds) {
    # 2. For each image, find any containers (running or stopped) using it.
    $containersToRemove = foreach ($id in $imageIds) {
        docker ps -a --filter "ancestor=$id" -q
    }

    if ($containersToRemove) {
        Write-Host "Found lingering containers based on image ancestry. Forcefully stopping and removing them..."
        docker stop $containersToRemove
        docker rm $containersToRemove
    } else {
        Write-Host "No lingering containers found based on image ancestry."
    }
}

# Remove all images built by docker-compose
Write-Host "Removing docker images..."
if ($imageIds) {
    Write-Host "Attempting to remove images: $($imageIds -join ', ')"
    docker rmi -f $imageIds
} else {
    Write-Host "No 'consilient' images found to remove."    
}

# Prune unused volumes and networks
Write-Host "Pruning unused volumes..."
docker volume prune -f

Write-Host "Pruning unused networks..."
docker network prune -f

Write-Host "Cleanup complete."
