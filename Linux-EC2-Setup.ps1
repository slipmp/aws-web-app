#Install Forró app on Linux- Amazon Linux 2 LTS AMI with .NET Core 2.1

#Elevate user to super user
sudo su
yum install git

#adding NPM as a source for yum
curl --silent --location https://rpm.nodesource.com/setup_10.x | bash -

#installing NPM as it is required to perform dotnet publish
yum install -y nodejs

#install Apache
yum install httpd -y

#going to the folder desired to install app
cd var
mkdir www
cd www

#cloning my repo and publish it
git clone https://github.com/slipmp/forro.git
cd forro/Forro.Admin/

sudo dotnet publish --configuration Release