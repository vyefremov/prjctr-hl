# Homework: AWS EC2 and Load Balancing

## Task

1. Create 2 new micro EC2 instances.
2. Create a new load balancer that distributes traffic between the 2 instances.

## Solution

#### Create 2 new micro EC2 instances.
   - Choose the Ubuntu Server
   - Choose the `t3.micro` instance type
   - Select `2` instances and click on `Review and Launch`

#### Create a new load balancer that distributes traffic between the 2 instances.
   - Choose the `Application Load Balancer`
   - Configure the load balancer
   - Register the targets
   - Review and create the load balancer

#### Configure the instances

```bash
sudo -i
apt update
apt install nginx
systemctl start nginx
systemctl status nginx
systemctl enable nginx

nano /var/www/html/index.nginx-debian.html
```

#### Created resources

- [Instance 1](http://ec2-13-48-42-19.eu-north-1.compute.amazonaws.com/)
- [Instance 2](http://ec2-13-53-177-159.eu-north-1.compute.amazonaws.com/)
- [Load Balancer](http://hw26-lb-24881898.eu-north-1.elb.amazonaws.com/)

> [ERROR] Failing to get response from the load balancer. Request just hangs.

### Load Balancer Configuration

```bash
aws elbv2 describe-load-balancers --query 'LoadBalancers[*]' --output json
```

```json
[
   {
      "LoadBalancerArn":"arn:aws:elasticloadbalancing:eu-north-1:381492205129:loadbalancer/app/hw26-lb/aec65eee821a2600",
      "DNSName":"hw26-lb-24881898.eu-north-1.elb.amazonaws.com",
      "CanonicalHostedZoneId":"Z23TAZ6LKFMNIO",
      "LoadBalancerName":"hw26-lb",
      "Scheme":"internet-facing",
      "VpcId":"vpc-0330269e3aead56ab",
      "State":{
         "Code":"active"
      },
      "Type":"application",
      "AvailabilityZones":[
         {
            "ZoneName":"eu-north-1c",
            "SubnetId":"subnet-027c8151ed4503a36",
            "LoadBalancerAddresses":[
               
            ]
         },
         {
            "ZoneName":"eu-north-1b",
            "SubnetId":"subnet-0ebf9fd10c6f0ba61",
            "LoadBalancerAddresses":[
               
            ]
         }
      ],
      "SecurityGroups":[
         "sg-0237fbc7c58f1d905"
      ],
      "IpAddressType":"ipv4"
   }
]
```

### EC2 Instances Configuration 

```bash
aws ec2 describe-instances --query 'Reservations[*].Instances[*]' --output json
```

```json
[
   [
      {
         "AmiLaunchIndex":0,
         "ImageId":"ami-0705384c0b33c194c",
         "InstanceId":"i-049860f9ee925467a",
         "InstanceType":"t3.micro",
         "KeyName":"hw26keypair",
         "Monitoring":{
            "State":"disabled"
         },
         "Placement":{
            "AvailabilityZone":"eu-north-1b",
            "GroupName":"",
            "Tenancy":"default"
         },
         "PrivateDnsName":"ip-172-31-42-78.eu-north-1.compute.internal",
         "PrivateIpAddress":"172.31.42.78",
         "ProductCodes":[
            
         ],
         "PublicDnsName":"ec2-13-48-42-19.eu-north-1.compute.amazonaws.com",
         "PublicIpAddress":"13.48.42.19",
         "State":{
            "Code":16,
            "Name":"running"
         },
         "StateTransitionReason":"",
         "SubnetId":"subnet-0ebf9fd10c6f0ba61",
         "VpcId":"vpc-0330269e3aead56ab",
         "Architecture":"x86_64",
         "BlockDeviceMappings":[
            {
               "DeviceName":"/dev/sda1",
               "Ebs":{
                  "DeleteOnTermination":true,
                  "Status":"attached",
                  "VolumeId":"vol-04818ffc0c7e8be45"
               }
            }
         ],
         "ClientToken":"7021d51d-dbb9-4b4b-a49f-4beb1f5e8c3a",
         "EbsOptimized":true,
         "EnaSupport":true,
         "Hypervisor":"xen",
         "NetworkInterfaces":[
            {
               "Association":{
                  "IpOwnerId":"amazon",
                  "PublicDnsName":"ec2-13-48-42-19.eu-north-1.compute.amazonaws.com",
                  "PublicIp":"13.48.42.19"
               },
               "Attachment":{
                  "AttachmentId":"eni-attach-0a31e11a7acbdb979",
                  "DeleteOnTermination":true,
                  "DeviceIndex":0,
                  "Status":"attached",
                  "NetworkCardIndex":0
               },
               "Description":"",
               "Groups":[
                  {
                     "GroupName":"launch-wizard-1",
                     "GroupId":"sg-0b67aa72f15a14feb"
                  }
               ],
               "Ipv6Addresses":[
                  
               ],
               "MacAddress":"0a:bc:36:bf:4b:2f",
               "NetworkInterfaceId":"eni-04081be024ad51606",
               "OwnerId":"381492205129",
               "PrivateDnsName":"ip-172-31-42-78.eu-north-1.compute.internal",
               "PrivateIpAddress":"172.31.42.78",
               "PrivateIpAddresses":[
                  {
                     "Association":{
                        "IpOwnerId":"amazon",
                        "PublicDnsName":"ec2-13-48-42-19.eu-north-1.compute.amazonaws.com",
                        "PublicIp":"13.48.42.19"
                     },
                     "Primary":true,
                     "PrivateDnsName":"ip-172-31-42-78.eu-north-1.compute.internal",
                     "PrivateIpAddress":"172.31.42.78"
                  }
               ],
               "SourceDestCheck":true,
               "Status":"in-use",
               "SubnetId":"subnet-0ebf9fd10c6f0ba61",
               "VpcId":"vpc-0330269e3aead56ab",
               "InterfaceType":"interface"
            }
         ],
         "RootDeviceName":"/dev/sda1",
         "RootDeviceType":"ebs",
         "SecurityGroups":[
            {
               "GroupName":"launch-wizard-1",
               "GroupId":"sg-0b67aa72f15a14feb"
            }
         ],
         "SourceDestCheck":true,
         "Tags":[
            {
               "Key":"Name",
               "Value":"hw26-ec2-1"
            }
         ],
         "VirtualizationType":"hvm",
         "CpuOptions":{
            "CoreCount":1,
            "ThreadsPerCore":2
         },
         "CapacityReservationSpecification":{
            "CapacityReservationPreference":"open"
         },
         "HibernationOptions":{
            "Configured":false
         },
         "MetadataOptions":{
            "State":"applied",
            "HttpTokens":"required",
            "HttpPutResponseHopLimit":2,
            "HttpEndpoint":"enabled",
            "HttpProtocolIpv6":"disabled",
            "InstanceMetadataTags":"disabled"
         },
         "EnclaveOptions":{
            "Enabled":false
         },
         "BootMode":"uefi-preferred",
         "PlatformDetails":"Linux/UNIX",
         "UsageOperation":"RunInstances",
         "PrivateDnsNameOptions":{
            "HostnameType":"ip-name",
            "EnableResourceNameDnsARecord":true,
            "EnableResourceNameDnsAAAARecord":false
         },
         "MaintenanceOptions":{
            "AutoRecovery":"default"
         },
         "CurrentInstanceBootMode":"uefi"
      },
      {
         "AmiLaunchIndex":1,
         "ImageId":"ami-0705384c0b33c194c",
         "InstanceId":"i-0866c0789c8ce438c",
         "InstanceType":"t3.micro",
         "KeyName":"hw26keypair",
         "Monitoring":{
            "State":"disabled"
         },
         "Placement":{
            "AvailabilityZone":"eu-north-1b",
            "GroupName":"",
            "Tenancy":"default"
         },
         "PrivateDnsName":"ip-172-31-33-143.eu-north-1.compute.internal",
         "PrivateIpAddress":"172.31.33.143",
         "ProductCodes":[
            
         ],
         "PublicDnsName":"ec2-13-53-177-159.eu-north-1.compute.amazonaws.com",
         "PublicIpAddress":"13.53.177.159",
         "State":{
            "Code":16,
            "Name":"running"
         },
         "StateTransitionReason":"",
         "SubnetId":"subnet-0ebf9fd10c6f0ba61",
         "VpcId":"vpc-0330269e3aead56ab",
         "Architecture":"x86_64",
         "BlockDeviceMappings":[
            {
               "DeviceName":"/dev/sda1",
               "Ebs":{
                  "DeleteOnTermination":true,
                  "Status":"attached",
                  "VolumeId":"vol-0eee321fe7c29d4a4"
               }
            }
         ],
         "ClientToken":"7021d51d-dbb9-4b4b-a49f-4beb1f5e8c3a",
         "EbsOptimized":true,
         "EnaSupport":true,
         "Hypervisor":"xen",
         "NetworkInterfaces":[
            {
               "Association":{
                  "IpOwnerId":"amazon",
                  "PublicDnsName":"ec2-13-53-177-159.eu-north-1.compute.amazonaws.com",
                  "PublicIp":"13.53.177.159"
               },
               "Attachment":{
                  "AttachmentId":"eni-attach-0653d42df87f7fc4e",
                  "DeleteOnTermination":true,
                  "DeviceIndex":0,
                  "Status":"attached",
                  "NetworkCardIndex":0
               },
               "Description":"",
               "Groups":[
                  {
                     "GroupName":"launch-wizard-1",
                     "GroupId":"sg-0b67aa72f15a14feb"
                  }
               ],
               "Ipv6Addresses":[
                  
               ],
               "MacAddress":"0a:6b:6e:99:e1:01",
               "NetworkInterfaceId":"eni-0612fb33064ac2a9f",
               "OwnerId":"381492205129",
               "PrivateDnsName":"ip-172-31-33-143.eu-north-1.compute.internal",
               "PrivateIpAddress":"172.31.33.143",
               "PrivateIpAddresses":[
                  {
                     "Association":{
                        "IpOwnerId":"amazon",
                        "PublicDnsName":"ec2-13-53-177-159.eu-north-1.compute.amazonaws.com",
                        "PublicIp":"13.53.177.159"
                     },
                     "Primary":true,
                     "PrivateDnsName":"ip-172-31-33-143.eu-north-1.compute.internal",
                     "PrivateIpAddress":"172.31.33.143"
                  }
               ],
               "SourceDestCheck":true,
               "Status":"in-use",
               "SubnetId":"subnet-0ebf9fd10c6f0ba61",
               "VpcId":"vpc-0330269e3aead56ab",
               "InterfaceType":"interface"
            }
         ],
         "RootDeviceName":"/dev/sda1",
         "RootDeviceType":"ebs",
         "SecurityGroups":[
            {
               "GroupName":"launch-wizard-1",
               "GroupId":"sg-0b67aa72f15a14feb"
            }
         ],
         "SourceDestCheck":true,
         "Tags":[
            {
               "Key":"Name",
               "Value":"hw26-ec2-1"
            }
         ],
         "VirtualizationType":"hvm",
         "CpuOptions":{
            "CoreCount":1,
            "ThreadsPerCore":2
         },
         "CapacityReservationSpecification":{
            "CapacityReservationPreference":"open"
         },
         "HibernationOptions":{
            "Configured":false
         },
         "MetadataOptions":{
            "State":"applied",
            "HttpTokens":"required",
            "HttpPutResponseHopLimit":2,
            "HttpEndpoint":"enabled",
            "HttpProtocolIpv6":"disabled",
            "InstanceMetadataTags":"disabled"
         },
         "EnclaveOptions":{
            "Enabled":false
         },
         "BootMode":"uefi-preferred",
         "PlatformDetails":"Linux/UNIX",
         "UsageOperation":"RunInstances",
         "PrivateDnsNameOptions":{
            "HostnameType":"ip-name",
            "EnableResourceNameDnsARecord":true,
            "EnableResourceNameDnsAAAARecord":false
         },
         "MaintenanceOptions":{
            "AutoRecovery":"default"
         },
         "CurrentInstanceBootMode":"uefi"
      }
   ]
]
```

