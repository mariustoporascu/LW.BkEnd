FROM node:18-alpine

# Stage 1: Install dependencies
RUN apk add --no-cache \
    bash curl \
    py3-pip \
    libreoffice \
    git

# fonts - https://wiki.alpinelinux.org/wiki/Fonts
RUN apk add --no-cache \
    font-noto font-noto-cjk font-noto-extra \
    terminus-font \
    ttf-font-awesome \
    ttf-dejavu \
    ttf-freefont \
    ttf-hack \
    ttf-inconsolata \
    ttf-liberation \
    ttf-mononoki  \
    ttf-opensans   \
    fontconfig && \
    fc-cache -f

RUN rm $(which wget) && \
    rm -rf /var/cache/apk/* /tmp/*

# renovate: datasource=repology depName=temurin-17-jdk versioning=loose
ARG VERSION_ADOPTIUM_TEMURIN="17.0.5_p8-r0"

# install Eclipse Temurin JDK
RUN curl https://packages.adoptium.net/artifactory/api/security/keypair/public/repositories/apk -o /etc/apk/keys/adoptium.rsa.pub && \
    echo 'https://packages.adoptium.net/artifactory/apk/alpine/main' >> /etc/apk/repositories && \
    apk update && apk add temurin-17-jdk=${VERSION_ADOPTIUM_TEMURIN}

#install unoserver
RUN git clone https://github.com/unoconv/unoserver.git /tmp/unoserver && \
    pip install --no-cache-dir /tmp/unoserver && \
    rm -rf /tmp/unoserver

# FIX: pyuno path not set  (https://gitlab.alpinelinux.org/alpine/aports/-/issues/13359)
# define path
ARG PATH_LO=/usr/lib/libreoffice/program
ARG PATH_SP=/usr/lib/python3.10/site-packages

RUN \
    # copy unohelper.py
    cp "$PATH_LO"/unohelper.py "$PATH_SP"/  && \
    # prefix path to uno.py
    echo -e "import sys, os\nsys.path.append('/usr/lib/libreoffice/program')\nos.putenv('URE_BOOTSTRAP', 'vnd.sun.star.pathname:/usr/lib/libreoffice/program/fundamentalrc')" > "$PATH_SP"/uno.py  && \
    # copy the original's content
    cat "$PATH_LO"/uno.py >> "$PATH_SP"/uno.py

# Init node.js app
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm install -g nodemon

# Expose port for the API
EXPOSE 80

# Start the API and the unoserver in parallel
CMD ["sh", "-c", "unoserver & nodemon --legacy-watch index.js"]