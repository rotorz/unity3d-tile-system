# Copyright (c) Rotorz Limited. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root.

PACKAGE_NAME := rotorz/unity3d-tile-system
PACKAGE_COPYRIGHT := Rotorz Limited. All rights reserved.
PACKAGE_LANG_CLASS := TileLang


EDITOR_DIR := ./Editor
LANGUAGES_DIR := ./Languages

LANGUAGE_SOURCES := $(shell find $(EDITOR_DIR) -type f \( -name "*.cs" -or -name "*.cs.template" \))
LANGUAGE_POT_OUTPUT := $(LANGUAGES_DIR)/language.pot


#
# Default Target: Build all.
#
all: $(LANGUAGE_POT_OUTPUT) update-product-info


#
# Target: Install npm
#
install-npm:

	npm install


#
# Target: Language .pot file.
#
$(LANGUAGE_POT_OUTPUT): $(LANGUAGE_SOURCES)

	# Produce a text file that lists all of the source files.
	find $(EDITOR_DIR) -type f | grep -E '\.cs(\.template)?$$' > $(LANGUAGES_DIR)/Strings.filelist

	# Extract strings from source files.
	xgettext --files-from=$(LANGUAGES_DIR)/Strings.filelist \
			 --from-code=UTF-8 \
			 --language=C# \
			 --keyword=$(PACKAGE_LANG_CLASS).Text \
			 --keyword=$(PACKAGE_LANG_CLASS).ParticularText:1c,2 \
			 --keyword=$(PACKAGE_LANG_CLASS).PluralText:1,2 \
			 --keyword=$(PACKAGE_LANG_CLASS).ParticularPluralText:1c,2,3 \
			 --keyword='$(PACKAGE_LANG_CLASS).ProperName:1,"PROPER NAME: This is a proper name. See the gettext manual, section Names."' \
			 --copyright-holder='$(PACKAGE_COPYRIGHT)' \
			 --package-name='$(PACKAGE_NAME)' \
			 --add-comments \
			 --output=$(LANGUAGE_POT_OUTPUT)

	# Remove the file that lists all of the source files.
	rm $(LANGUAGES_DIR)/Strings.filelist

	# Use context text of "Proper Name" for strings extracted from `$(PACKAGE_LANG_CLASS).ProperName`.
	perl -p0777i -e 's/(#\. )PROPER NAME: (.*?)(msgid ")/\1\2msgctxt "Proper Name"\n\3/sg' $(LANGUAGE_POT_OUTPUT)
	rm $(LANGUAGE_POT_OUTPUT).bak


#
# Target: Cleanup previous build.
#
clean: clean-temp-files

	# Remove output language .pot file.
	rm $(LANGUAGE_POT_OUTPUT)


#
# Target: Cleanup temporary files.
#
clean-temp-files:

	# Remove temporary output folder.
	rm -rf ./temp


#
# Target: Update ProductInfo.cs
#
update-product-info:

	# Prepare the solution and project files for MSBuild.
	node ./scripts/update-productinfo.js


#
# Target: Build legacy DLLs.
#
build-legacy-dlls: clean-temp-files install-npm update-product-info

	# Prepare the solution and project files for MSBuild.
	node ./scripts/prepare-solution.js

	# Build the DLLs using MSBuild.
	MSBuild.exe temp\TileSystem.sln /t:Build /p:Configuration=Release

	# Reveal the output folder.
	explorer temp\Deploy


#
# Target: Build Wiki
#
build-wiki: install-npm

	# Prepare temporary directory for wiki repository.
	rm -rf temp/wiki
	mkdir -p temp

	# Clone wiki repository.
	git clone $(WIKI_REPOSITORY_URL) temp/wiki

	# Clean previous build of documentation.
	npm run clean-wiki-output -- --input="$(DOCS_BOOK_YAML_PATH)" --output=temp/wiki
	# Build documentation.
	npm run build-wiki-output -- --input="$(DOCS_BOOK_YAML_PATH)" --output=temp/wiki

	# Commit and push changes to the wiki repository.
	git -C temp/wiki add . \
	  && git -C temp/wiki commit -m "Updated documentation." \
	  && git -C temp/wiki push \
	  ; true

	# Cleanup.
	rm -rf temp/wiki
	rmdir temp; true

	@echo ""
	@echo "Wiki updated from documentation files."
	@echo ""
