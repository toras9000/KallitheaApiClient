FROM toras9000/kallithea-mp:0.7.0

ARG DEBIAN_FRONTEND=noninteractive

ARG KALLITHEA_REV=stable

RUN hg clone --rev "$KALLITHEA_REV" https://kallithea-scm.org/repos/kallithea /kallithea/src \
 && cp -RT /kallithea/src/kallithea "$(su-exec kallithea:kallithea python3 -m site --user-site)/kallithea" \
 && rm -rf /kallithea/src
 
