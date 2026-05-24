GCS_KEY_PATH := Configs/gcs.json

.PHONY: github run scan

run:
	@GOOGLE_APPLICATION_CREDENTIALS=$(GCS_KEY_PATH) dotnet run

scan:
	./Scripts/container-scan.sh

github:
	@if [ -z "$(CM)" ]; then \
		echo "Usage: make github CM=\"commit message\""; \
		exit 1; \
	fi
	git add .
	git commit -m "$(CM)"
	git push origin main
