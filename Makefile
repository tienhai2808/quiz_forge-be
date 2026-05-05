.PHONY: github

github:
	@if [ -z "$(CM)" ]; then \
		echo "Usage: make github CM=\"commit message\""; \
		exit 1; \
	fi
	git add .
	git commit -m "$(CM)"
	git push origin main