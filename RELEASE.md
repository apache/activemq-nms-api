
replace \<rc-version\> in the below steps with your rc version, e.g. 1.8.0-rc3

git clone https://gitbox.apache.org/repos/asf/activemq-nms-api.git

ensure versions (csproj files, and package.ps1)

build project

git tag -m "tag \<rc-version\>" \<rc-version\>
git push origin \<rc-version\>

svn checkout rc dist dev https://dist.apache.org/repos/dist/dev/activemq/activemq-nms-api

create folder for rc, e.g. mkdir \<rc-version\>

copy all contents from package dir from project into the rc folder.

sign the files, you will find a helper script "sign-release.sh", invoke it as

 ./sign-release.sh \<rc-version\> \<your-gpg-password\>
 
commit all files into the rc dist dev repo.




