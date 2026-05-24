#!/bin/bash 

echo "Building..."
docker build -t quiz_forge-be:local . 
echo "Scanning..." 
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock -v ~/.cache:/root/.cache aquasec/trivy:0.69.3 image quiz_forge-be:local
