# Homework: AWS Autoscale Groups

## Task

- Create autoscale group that will contain one on demand instance and will scale on spot instances.
- Set up scaling policy based on AVG CPU usage
- Set up scaling policy based on requests amount that allows non-linear growth

## Solution

1. Create a new EC2 instance or reuse an existing one from the previous homework **[hw26-aws-ec2](../hw26-aws-ec2)**
2. Create a new AMI from the instance
3. Apply Terraform configuration from the **[main.tf](./terraform/main.tf)** file

### Terraform Essentials

#### Launch Template

```tf
variable "instance_type" {
  default = "t3.micro"
}

variable "ami_id" {
  default = "ami-0c29cfccd1b73dfd7"
}

resource "aws_launch_template" "hw28-launch-template" {
  name_prefix = "hw28-"
  image_id = var.ami_id
  instance_type = var.instance_type
}
```

#### Autoscaling Group

Contains on demand instance and scales spot instances.

```tf
resource "aws_autoscaling_group" "hw28-autoscaling-group" {
  desired_capacity = 1 # Number of instances to launch
  min_size = 1
  max_size = 10

  vpc_zone_identifier  = ["subnet-0ebf9fd10c6f0ba61", "subnet-027c8151ed4503a36"]
  target_group_arns = [aws_lb_target_group.hw28-load-balancer-target-group.arn]

  mixed_instances_policy {
    launch_template {
      launch_template_specification {
        launch_template_id = aws_launch_template.hw28-launch-template.id
        version = "$Latest"
      }
    }

    instances_distribution {
      on_demand_base_capacity = 1 # Absolute minimum amount of desired capacity that must be fulfilled by on-demand instances
      on_demand_percentage_above_base_capacity = 0 # Percentage of additional on-demand instances above the base capacity, ensure to only use spot instances
      spot_allocation_strategy = "lowest-price"
    }
  }
}
```

#### Autoscaling Policy: Scale Up Linear

```tf
resource "aws_autoscaling_policy" "hw28-autoscaling-policy-scale-up-linear" {
  name = "hw28-autoscaling-policy-scale-up-linear"
  autoscaling_group_name = aws_autoscaling_group.hw28-autoscaling-group.name
  scaling_adjustment = 1 # Number of instances by which to scale.
  adjustment_type = "ChangeInCapacity" # Whether the adjustment is an absolute number or a percentage of the current capacity. Values: ChangeInCapacity, PercentChangeInCapacity, ExactCapacity
  cooldown = 300 # Amount of time, in seconds, after a scaling activity completes and before the next scaling activity can start.
}
```

#### Autoscaling Policy: Scale Down Linear

```tf
resource "aws_autoscaling_policy" "hw28-autoscaling-policy-scale-down-linear" {
  name = "hw28-autoscaling-policy-scale-down-linear"
  scaling_adjustment = -1
  adjustment_type = "ChangeInCapacity"
  cooldown = 300
  autoscaling_group_name = aws_autoscaling_group.hw28-autoscaling-group.name
}
```

#### Autoscaling Policy: Scale Up Non-Linear

It uses step scaling policy to scale up based on requests count.
I'm not sure if it's the best way to scale up based on requests count, but it's a good example of how to use step scaling policy.
We also can set policy type to simple scaling and adjustment type to `PercentChangeInCapacity` which will also result in non-linear growth.

```tf
resource "aws_autoscaling_policy" "hw28-autoscaling-policy-scale-up-by-requests-count" {
  name = "hw28-autoscaling-policy-scale-up-by-requests-count"
  policy_type = "StepScaling"
  autoscaling_group_name = aws_autoscaling_group.hw28-autoscaling-group.name
  adjustment_type = "ChangeInCapacity" # Whether the adjustment is an absolute number or a percentage of the current capacity. Values: ChangeInCapacity, PercentChangeInCapacity, ExactCapacity

  step_adjustment {
    metric_interval_lower_bound = 0
    metric_interval_upper_bound = 2000
    scaling_adjustment = 2
  }

  step_adjustment {
    metric_interval_lower_bound = 2000
    metric_interval_upper_bound = 10000
    scaling_adjustment = 4
  }

  step_adjustment {
    metric_interval_lower_bound = 10000
    scaling_adjustment = 8
  }
}
```

#### CloudWatch Metric Alarm: CPU Usage >= 50%

```tf
resource "aws_cloudwatch_metric_alarm" "hw28-cpu-alarm" {
    alarm_name = "hw28-cpu-alarm"
    comparison_operator = "GreaterThanOrEqualToThreshold"
    evaluation_periods = 1 # The number of periods over which data is compared to the specified threshold
    metric_name = "CPUUtilization"
    namespace = "AWS/EC2"
    period = 60 # The period in seconds over which the specified statistic is applied. Valid values are 10, 30, or any multiple of 60.
    statistic = "Average"
    threshold = 50 # The value against which the specified statistic is compared. This parameter is required for alarms based on static thresholds, but should not be used for alarms based on anomaly detection models.
    alarm_description = "This metric monitors the CPU utilization"
    alarm_actions = [aws_autoscaling_policy.hw28-autoscaling-policy-scale-up-linear.arn]
    dimensions = {
        AutoScalingGroupName = aws_autoscaling_group.hw28-autoscaling-group.name
    }
}
```

#### CloudWatch Metric Alarm: Requests Count >= 1000

```tf
resource "aws_cloudwatch_metric_alarm" "hw28-high-request-count-alarm" {
  alarm_name          = "hw28-high-request-count-alarm-v2"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "RequestCount"
  namespace           = "AWS/ApplicationELB"
  period              = 60 # In seconds.
  statistic           = "Sum"
  threshold           = 1000

  dimensions = {
    LoadBalancer = aws_lb.hw28-load-balancer.name
  }

  alarm_actions = [aws_autoscaling_policy.hw28-autoscaling-policy-scale-up-by-requests-count.arn]
}
```

#### CloudWatch Metric Alarm: Requests Count <= 100

Used to scale down the number of instances.

```tf
resource "aws_cloudwatch_metric_alarm" "hw28-low-request-count-alarm" {
  alarm_name          = "hw28-low-request-count-alarm"
  comparison_operator = "LessThanThreshold"
  evaluation_periods  = 10 # The number of periods over which data is compared to the specified threshold
  metric_name         = "RequestCount"
  namespace           = "AWS/ApplicationELB"
  period              = 60 # In seconds.
  statistic           = "Sum"
  threshold           = 50

  dimensions = {
    LoadBalancer = aws_lb.hw28-load-balancer.name
  }

  alarm_actions = [aws_autoscaling_policy.hw28-autoscaling-policy-scale-down-linear.arn]
}
```
