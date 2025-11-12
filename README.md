# Azure AKS Infrastructure - Production-Ready Kubernetes Platform

A comprehensive, production-ready Infrastructure as Code (IaC) repository for deploying and managing Azure Kubernetes Service (AKS) with enterprise-grade components using Terraform, ArgoCD, and Helm.

[![Terraform](https://img.shields.io/badge/Terraform-≥1.5-623CE4?logo=terraform)](https://www.terraform.io/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-≥1.28-326CE5?logo=kubernetes)](https://kubernetes.io/)
[![ArgoCD](https://img.shields.io/badge/ArgoCD-≥2.9-EF7B4D?logo=argo)](https://argoproj.github.io/cd/)
[![Azure](https://img.shields.io/badge/Azure-AKS-0078D4?logo=microsoft-azure)](https://azure.microsoft.com/en-us/services/kubernetes-service/)

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Components](#components)
- [Prerequisites](#prerequisites)
- [Repository Structure](#repository-structure)
- [Quick Start](#quick-start)
- [Detailed Deployment](#detailed-deployment)
- [API Gateway Options](#api-gateway-options)
- [Configuration](#configuration)
- [Monitoring & Observability](#monitoring--observability)
- [Security](#security)
- [Cost Optimization](#cost-optimization)
- [Troubleshooting](#troubleshooting)
- [Maintenance](#maintenance)
- [Contributing](#contributing)

## 🎯 Overview

This repository provides a complete, production-ready infrastructure for deploying enterprise applications on Azure Kubernetes Service. It follows GitOps principles and implements industry best practices for security, scalability, and observability.

### Key Features

- ✅ **Infrastructure as Code** - Complete Terraform modules for Azure resources
- ✅ **GitOps Workflow** - ArgoCD for continuous deployment and synchronization
- ✅ **Multi-Environment** - Separate configurations for dev, staging, and production
- ✅ **High Availability** - Multi-zone AKS cluster with auto-scaling
- ✅ **Observability Stack** - Prometheus, Grafana, and Loki for comprehensive monitoring
- ✅ **API Gateway** - Choice between Istio Service Mesh or Kong API Gateway
- ✅ **Data Layer** - PostgreSQL HA, Redis Cluster, and RabbitMQ
- ✅ **Security** - Cert-manager for TLS, RBAC, network policies, and secrets management
- ✅ **CI/CD Ready** - GitHub Actions workflows included

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                            Azure Cloud                                   │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                     Resource Group                                 │  │
│  │  ┌─────────────────────────────────────────────────────────────┐  │  │
│  │  │                 AKS Cluster (Multi-Zone)                     │  │  │
│  │  │                                                               │  │  │
│  │  │  ┌────────────────────────────────────────────────────────┐  │  │  │
│  │  │  │              Istio/Kong API Gateway                     │  │  │  │
│  │  │  │         (LoadBalancer with Public IP)                   │  │  │  │
│  │  │  └─────────────────┬──────────────────────────────────────┘  │  │  │
│  │  │                    │                                          │  │  │
│  │  │  ┌─────────────────┴──────────────────────────────────────┐  │  │  │
│  │  │  │            Application Workloads                        │  │  │  │
│  │  │  │  • Your Microservices                                   │  │  │  │
│  │  │  │  • ArajeezERP APIs                                      │  │  │  │
│  │  │  │  • AI Store Manager                                     │  │  │  │
│  │  │  └──────────────────────────────────────────────────────┬─┘  │  │  │
│  │  │                                                           │    │  │  │
│  │  │  ┌────────────────────────────────────────────────────┐  │    │  │  │
│  │  │  │         Middleware Layer                           │  │    │  │  │
│  │  │  │  ┌──────────┐  ┌──────────┐  ┌─────────────┐     │  │    │  │  │
│  │  │  │  │ RabbitMQ │  │  Redis   │  │ PostgreSQL  │     │  │    │  │  │
│  │  │  │  │ Cluster  │  │ Cluster  │  │  HA Cluster │     │  │    │  │  │
│  │  │  │  │ (3 nodes)│  │ (6 nodes)│  │  (3 nodes)  │     │  │    │  │  │
│  │  │  │  └──────────┘  └──────────┘  └─────────────┘     │  │    │  │  │
│  │  │  └────────────────────────────────────────────────────┘  │    │  │  │
│  │  │                                                           │    │  │  │
│  │  │  ┌────────────────────────────────────────────────────┐  │    │  │  │
│  │  │  │       Observability Stack                          │  │    │  │  │
│  │  │  │  ┌────────────┐  ┌──────────┐  ┌──────────────┐  │  │    │  │  │
│  │  │  │  │ Prometheus │  │ Grafana  │  │     Loki     │  │  │    │  │  │
│  │  │  │  │  + Alert   │  │          │  │  + Promtail  │  │  │    │  │  │
│  │  │  │  │  Manager   │  │          │  │              │  │  │    │  │  │
│  │  │  │  └────────────┘  └──────────┘  └──────────────┘  │  │    │  │  │
│  │  │  └────────────────────────────────────────────────────┘  │    │  │  │
│  │  │                                                           │    │  │  │
│  │  │  ┌────────────────────────────────────────────────────┐  │    │  │  │
│  │  │  │         Infrastructure                             │  │    │  │  │
│  │  │  │  • ArgoCD (GitOps)                                 │  │    │  │  │
│  │  │  │  • Cert-Manager (TLS)                              │  │    │  │  │
│  │  │  │  • Sealed Secrets                                  │  │    │  │  │
│  │  │  │  • Ingress Controller                              │  │    │  │  │
│  │  │  └────────────────────────────────────────────────────┘  │    │  │  │
│  │  │                                                              │  │  │
│  │  └──────────────────────────────────────────────────────────────┘  │  │
│  │                                                                      │  │
│  │  ┌────────────────┐  ┌─────────────────┐  ┌─────────────────┐     │  │
│  │  │  Azure CNI     │  │  Azure Monitor  │  │   Key Vault     │     │  │
│  │  │  Network       │  │  & Log Analytics│  │   Integration   │     │  │
│  │  └────────────────┘  └─────────────────┘  └─────────────────┘     │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  ┌────────────────┐  ┌─────────────────┐  ┌─────────────────────────┐    │
│  │ Container      │  │  Azure Storage  │  │  Terraform State        │    │
│  │ Registry (ACR) │  │  (Loki chunks)  │  │  Storage Account        │    │
│  └────────────────┘  └─────────────────┘  └─────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 📦 Components

| Component | Version | Purpose |
|-----------|---------|---------|
| **AKS** | 1.28+ | Managed Kubernetes cluster |
| **ArgoCD** | 2.9+ | GitOps continuous delivery |
| **Istio** | 1.20+ | Service mesh & API gateway |
| **Kong** | 2.35+ | Alternative API gateway |
| **Prometheus Stack** | 54.2+ | Metrics collection & alerting |
| **Grafana** | Latest | Metrics visualization |
| **Loki** | 2.10+ | Log aggregation |
| **RabbitMQ** | 3.11+ | Message broker cluster |
| **Redis** | 9.5+ | In-memory data store cluster |
| **PostgreSQL** | 12.3+ | Relational database HA cluster |
| **Cert-Manager** | 1.13+ | TLS certificate management |
| **Sealed Secrets** | Latest | Encrypted secrets in Git |

## 🔧 Prerequisites

### Required Tools

```bash
# Azure CLI
az version  # >= 2.50.0

# Terraform
terraform version  # >= 1.5.0

# kubectl
kubectl version --client  # >= 1.28.0

# Helm
helm version  # >= 3.12.0

# ArgoCD CLI (optional)
argocd version  # >= 2.9.0
```

### Azure Requirements

- Active Azure subscription
- Sufficient quota for:
  - Virtual machines (Standard_D4s_v5, Standard_D8s_v5)
  - Public IP addresses
  - Load balancers
- Azure Active Directory admin access (for RBAC)
- Service Principal or Managed Identity for Terraform

### Azure Permissions

Your account needs:
- `Contributor` role on the subscription
- `User Access Administrator` role for RBAC configuration
- Ability to create service principals

## 📁 Repository Structure

```
azure-aks-infrastructure/
├── terraform/                      # Infrastructure as Code
│   ├── environments/              # Environment-specific configs
│   │   ├── dev/
│   │   │   ├── main.tf           # Dev environment definition
│   │   │   ├── variables.tf      # Variable declarations
│   │   │   ├── terraform.tfvars  # Dev-specific values
│   │   │   └── backend.tf        # Remote state configuration
│   │   ├── staging/
│   │   └── production/
│   └── modules/                   # Reusable Terraform modules
│       ├── aks/                  # AKS cluster module
│       ├── networking/           # VNet, subnets, NSGs
│       ├── acr/                  # Container registry
│       └── storage/              # Storage accounts
│
├── helm-charts/                   # Helm chart configurations
│   ├── infrastructure/           # Core infrastructure
│   │   ├── argocd/
│   │   ├── cert-manager/
│   │   ├── ingress-nginx/
│   │   └── sealed-secrets/
│   ├── observability/            # Monitoring stack
│   │   ├── kube-prometheus-stack/
│   │   └── loki-stack/
│   ├── middleware/               # Message brokers & caches
│   │   ├── rabbitmq-cluster/
│   │   └── redis-cluster/
│   ├── databases/                # Database clusters
│   │   └── postgresql-ha/
│   └── api-gateway/              # API gateway options
│       ├── istio/
│       └── kong/
│
├── argocd/                        # ArgoCD applications
│   ├── bootstrap/
│   │   └── root-app.yaml         # App of apps pattern
│   ├── applications/             # Application definitions
│   │   ├── infrastructure/
│   │   ├── observability/
│   │   ├── middleware/
│   │   └── databases/
│   └── projects/                 # ArgoCD projects
│       ├── infrastructure.yaml
│       ├── observability.yaml
│       └── data.yaml
│
├── manifests/                     # Kubernetes manifests
│   ├── istio/                    # Istio configurations
│   │   ├── gateways/
│   │   ├── virtual-services/
│   │   ├── destination-rules/
│   │   └── policies/
│   └── kong/                     # Kong configurations
│       ├── kong-plugins/
│       └── ingresses/
│
├── scripts/                       # Automation scripts
│   ├── setup-terraform.sh        # Initialize Terraform
│   ├── deploy-argocd.sh         # Deploy ArgoCD
│   ├── generate-secrets.sh      # Generate secrets
│   ├── choose-gateway.sh        # Gateway selection helper
│   └── destroy.sh               # Cleanup script
│
├── .github/
│   └── workflows/                # CI/CD pipelines
│       ├── terraform-plan.yml   # Terraform plan on PR
│       ├── terraform-apply.yml  # Terraform apply on merge
│       └── helm-lint.yml        # Helm chart validation
│
├── docs/                          # Documentation
│   ├── architecture.md           # Architecture decisions
│   ├── deployment-guide.md      # Detailed deployment guide
│   ├── api-gateway-comparison.md # Istio vs Kong
│   └── runbooks/                # Operational runbooks
│       ├── scaling.md
│       ├── backup-restore.md
│       └── disaster-recovery.md
│
├── .gitignore
├── README.md                     # This file
└── LICENSE
```

## 🚀 Quick Start

### 1. Clone Repository

```bash
git clone https://github.com/your-org/azure-aks-infrastructure.git
cd azure-aks-infrastructure
```

### 2. Configure Azure Authentication

```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "your-subscription-id"

# Create service principal for Terraform
az ad sp create-for-rbac \
  --name "terraform-aks-sp" \
  --role Contributor \
  --scopes /subscriptions/your-subscription-id

# Export credentials
export ARM_CLIENT_ID="<appId>"
export ARM_CLIENT_SECRET="<password>"
export ARM_SUBSCRIPTION_ID="<subscription-id>"
export ARM_TENANT_ID="<tenant>"
```

### 3. Initialize Terraform Backend

```bash
cd scripts
./setup-terraform.sh
```

### 4. Deploy Infrastructure

```bash
cd ../terraform/environments/dev

# Initialize Terraform
terraform init

# Review changes
terraform plan -out=tfplan

# Apply changes
terraform apply tfplan
```

### 5. Connect to AKS

```bash
# Get AKS credentials
az aks get-credentials \
  --resource-group rg-aks-dev-eastus \
  --name aks-dev-eastus

# Verify connection
kubectl get nodes
```

### 6. Deploy ArgoCD

```bash
cd ../../../scripts
./deploy-argocd.sh
```

### 7. Access ArgoCD UI

```bash
# Port forward
kubectl port-forward svc/argocd-server -n argocd 8080:443

# Get admin password
kubectl -n argocd get secret argocd-initial-admin-secret \
  -o jsonpath="{.data.password}" | base64 -d

# Open browser
open https://localhost:8080
```

### 8. Deploy Applications

```bash
# Apply root application (App of Apps)
kubectl apply -f ../argocd/bootstrap/root-app.yaml

# Watch ArgoCD sync all applications
argocd app list
```

## 📖 Detailed Deployment

### Step 1: Prepare Environment

```bash
# Create terraform.tfvars for your environment
cd terraform/environments/production

cat > terraform.tfvars <<EOF
# Azure Configuration
location                = "eastus"
environment            = "production"
resource_group_name    = "rg-aks-prod-eastus"

# AKS Configuration
kubernetes_version     = "1.28.3"
admin_group_object_ids = ["<your-aad-group-id>"]

# Node Pool Configuration
system_node_count      = 3
system_node_size       = "Standard_D4s_v5"
system_node_min_count  = 3
system_node_max_count  = 6

workload_node_count     = 3
workload_node_size      = "Standard_D8s_v5"
workload_node_min_count = 3
workload_node_max_count = 10

# Network Configuration
vnet_address_space = ["10.0.0.0/16"]
service_cidr       = "10.1.0.0/16"
dns_service_ip     = "10.1.0.10"

# Tags
tags = {
  Environment = "Production"
  ManagedBy   = "Terraform"
  Project     = "AKS-Infrastructure"
  Owner       = "DevOps-Team"
}
EOF
```

### Step 2: Review Terraform Plan

```bash
terraform init
terraform plan -out=tfplan

# Review the plan carefully
terraform show tfplan
```

### Step 3: Apply Infrastructure

```bash
terraform apply tfplan

# This will create:
# - Resource Group
# - Virtual Network & Subnets
# - AKS Cluster with 2 node pools
# - Azure Container Registry
# - Log Analytics Workspace
# - Storage Account for Loki
# - Public IP for LoadBalancer
```

### Step 4: Configure kubectl

```bash
# Get credentials
az aks get-credentials \
  --resource-group rg-aks-prod-eastus \
  --name aks-prod-eastus \
  --overwrite-existing

# Test connection
kubectl cluster-info
kubectl get nodes -o wide
```

### Step 5: Deploy Infrastructure Components

```bash
# Deploy ArgoCD
cd scripts
./deploy-argocd.sh

# Wait for ArgoCD to be ready
kubectl wait --for=condition=available \
  --timeout=300s \
  deployment/argocd-server -n argocd

# Deploy all applications via App of Apps
kubectl apply -f ../argocd/bootstrap/root-app.yaml
```

### Step 6: Configure Secrets

```bash
# Generate and apply secrets
./generate-secrets.sh

# This creates:
# - PostgreSQL passwords
# - RabbitMQ credentials
# - Redis passwords
# - Grafana admin password
# - Kong admin token
```

### Step 7: Choose API Gateway

```bash
# Run the interactive gateway selection
./choose-gateway.sh

# Options:
# 1. Istio - For service mesh capabilities
# 2. Kong - For traditional API gateway
# 3. Both - Istio internal + Kong external
```

### Step 8: Verify Deployment

```bash
# Check all ArgoCD applications
argocd app list

# Check all pods
kubectl get pods --all-namespaces

# Check services
kubectl get svc --all-namespaces
```

## 🌐 API Gateway Options

### Option 1: Istio Service Mesh (Recommended)

**Use When:**
- Multiple microservices (10+)
- Need mTLS between services
- Advanced traffic management required
- Deep observability needed

**Deploy:**
```bash
kubectl apply -f argocd/applications/infrastructure/istio-base.yaml
kubectl apply -f argocd/applications/infrastructure/istio-istiod.yaml
kubectl apply -f argocd/applications/infrastructure/istio-gateway.yaml
```

**Access:**
```bash
# Get LoadBalancer IP
kubectl get svc -n istio-ingress istio-ingressgateway

# Configure DNS
# api.yourdomain.com -> <LoadBalancer-IP>
```

**Example Virtual Service:**
```yaml
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: my-api
spec:
  hosts:
    - api.yourdomain.com
  gateways:
    - istio-ingress/main-gateway
  http:
    - route:
        - destination:
            host: my-service
            port:
              number: 80
```

### Option 2: Kong API Gateway

**Use When:**
- Traditional API gateway pattern
- External API management
- Developer portal needed
- Simpler deployment preferred

**Deploy:**
```bash
kubectl apply -f argocd/applications/infrastructure/kong-gateway.yaml
```

**Access:**
```bash
# Get LoadBalancer IP
kubectl get svc -n kong kong-proxy

# Access Kong Manager
kubectl port-forward -n kong svc/kong-admin 8001:8001

# Kong Manager UI
open http://localhost:8001
```

**Example Ingress:**
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: my-api
  annotations:
    konghq.com/plugins: rate-limiting,cors
spec:
  ingressClassName: kong
  rules:
    - host: api.yourdomain.com
      http:
        paths:
          - path: /
            backend:
              service:
                name: my-service
                port:
                  number: 80
```

### Comparison Table

| Feature | Istio | Kong |
|---------|-------|------|
| **Service Mesh** | ✅ Full mesh | ❌ API Gateway only |
| **mTLS** | ✅ Automatic | ⚠️ Manual setup |
| **Traffic Splitting** | ✅ Advanced | ✅ Basic |
| **Rate Limiting** | ✅ Via EnvoyFilter | ✅ Native plugin |
| **Learning Curve** | Steep | Moderate |
| **Resource Usage** | High | Medium |
| **Plugin Ecosystem** | Limited | Rich |
| **Admin UI** | ❌ None | ✅ Excellent |
| **Best For** | Microservices | API Management |

## ⚙️ Configuration

### Environment Variables

Create a `.env` file for sensitive values:

```bash
# Azure
export ARM_CLIENT_ID="..."
export ARM_CLIENT_SECRET="..."
export ARM_SUBSCRIPTION_ID="..."
export ARM_TENANT_ID="..."

# ArgoCD
export ARGOCD_ADMIN_PASSWORD="..."

# Grafana
export GRAFANA_ADMIN_PASSWORD="..."

# PostgreSQL
export POSTGRES_PASSWORD="..."

# RabbitMQ
export RABBITMQ_PASSWORD="..."
export RABBITMQ_ERLANG_COOKIE="..."

# Redis
export REDIS_PASSWORD="..."

# Kong
export KONG_ADMIN_TOKEN="..."
```

### Terraform Variables

Key variables in `terraform.tfvars`:

```hcl
# Location and naming
location            = "eastus"
environment         = "production"
resource_group_name = "rg-aks-prod-eastus"

# AKS
kubernetes_version  = "1.28.3"
dns_prefix          = "aks-prod"

# Node pools
system_node_count   = 3
system_node_size    = "Standard_D4s_v5"
workload_node_size  = "Standard_D8s_v5"

# Networking
vnet_address_space  = ["10.0.0.0/16"]
service_cidr        = "10.1.0.0/16"
dns_service_ip      = "10.1.0.10"

# Features
enable_azure_policy           = true
enable_oms_agent             = true
enable_key_vault_secrets     = true
enable_workload_identity     = true
```

### ArgoCD Project Configuration

```yaml
# argocd/projects/infrastructure.yaml
apiVersion: argoproj.io/v1alpha1
kind: AppProject
metadata:
  name: infrastructure
  namespace: argocd
spec:
  description: Infrastructure components
  
  sourceRepos:
    - '*'
  
  destinations:
    - namespace: '*'
      server: https://kubernetes.default.svc
  
  clusterResourceWhitelist:
    - group: '*'
      kind: '*'
  
  namespaceResourceWhitelist:
    - group: '*'
      kind: '*'
```

## 📊 Monitoring & Observability

### Access Dashboards

#### Grafana
```bash
# Port forward
kubectl port-forward -n observability svc/kube-prometheus-stack-grafana 3000:80

# Open browser
open http://localhost:3000

# Login
# Username: admin
# Password: <from secret>
```

**Pre-installed Dashboards:**
- Kubernetes Cluster Monitoring
- Node Exporter Full
- Istio Mesh Dashboard (if Istio installed)
- Kong Dashboard (if Kong installed)
- PostgreSQL Database
- RabbitMQ Overview
- Redis Overview

#### Prometheus
```bash
kubectl port-forward -n observability svc/kube-prometheus-stack-prometheus 9090:9090
open http://localhost:9090
```

#### AlertManager
```bash
kubectl port-forward -n observability svc/kube-prometheus-stack-alertmanager 9093:9093
open http://localhost:9093
```

### Key Metrics

**Cluster Health:**
```promql
# Node CPU usage
instance:node_cpu_utilisation:rate5m

# Node memory usage
instance:node_memory_utilisation:rate5m

# Pod CPU throttling
sum(rate(container_cpu_cfs_throttled_seconds_total[5m])) by (namespace, pod)
```

**Application Metrics:**
```promql
# Request rate
sum(rate(istio_requests_total[5m])) by (destination_service)

# Error rate
sum(rate(istio_requests_total{response_code=~"5.."}[5m])) by (destination_service)

# Latency (p99)
histogram_quantile(0.99, 
  sum(rate(istio_request_duration_milliseconds_bucket[5m])) by (le, destination_service)
)
```

### Logging

**View logs with kubectl:**
```bash
# Application logs
kubectl logs -n production deployment/my-app -f

# All pods in namespace
kubectl logs -n production -l app=my-app --all-containers=true -f
```

**Query Loki via Grafana:**
```logql
# All logs from namespace
{namespace="production"}

# Error logs
{namespace="production"} |= "error" | json

# Filter by pod
{namespace="production", pod=~"my-app-.*"}
```

## 🔒 Security

### Network Policies

```yaml
# Example: Restrict PostgreSQL access
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: postgresql-netpol
  namespace: databases
spec:
  podSelector:
    matchLabels:
      app: postgresql
  policyTypes:
    - Ingress
  ingress:
    - from:
        - namespaceSelector:
            matchLabels:
              name: production
      ports:
        - protocol: TCP
          port: 5432
```

### RBAC

```bash
# Create developer role
kubectl apply -f - <<EOF
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: developer
  namespace: production
rules:
  - apiGroups: ["", "apps", "batch"]
    resources: ["*"]
    verbs: ["get", "list", "watch"]
  - apiGroups: [""]
    resources: ["pods/log"]
    verbs: ["get"]
EOF
```

### Secrets Management

**Using Sealed Secrets:**
```bash
# Install kubeseal CLI
brew install kubeseal

# Create sealed secret
kubectl create secret generic my-secret \
  --from-literal=password=mysecret \
  --dry-run=client -o yaml | \
  kubeseal -o yaml > sealed-secret.yaml

# Apply sealed secret
kubectl apply -f sealed-secret.yaml
```

### TLS Certificates

**Let's Encrypt with cert-manager:**
```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@yourdomain.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
      - http01:
          ingress:
            class: nginx
```

## 💰 Cost Optimization

### Estimated Monthly Costs (Production)

| Resource | Instance | Count | Monthly Cost (USD) |
|----------|----------|-------|-------------------|
| AKS Control Plane | Managed | 1 | $73 |
| System Nodes | D4s_v5 | 3 | $420 |
| Workload Nodes | D8s_v5 | 3 | $840 |
| Load Balancer | Standard | 1 | $18 |
| Public IP | Standard | 1 | $4 |
| Storage (Premium SSD) | 500GB | - | $75 |
| Log Analytics | 50GB/day | - | $150 |
| **Total** | | | **~$1,580/month** |

### Cost Reduction Tips

1. **Use Spot Instances** for non-critical workloads:
```hcl
# Add to node pool configuration
priority        = "Spot"
eviction_policy = "Delete"
spot_max_price  = -1  # Use current spot price
```

2. **Enable Cluster Autoscaler** to scale down during off-hours

3. **Use Azure Reserved Instances** (1-year = 40% savings, 3-year = 60% savings)

4. **Optimize Storage**:
   - Use Standard SSD instead of Premium for non-critical data
   - Enable lifecycle management on storage accounts
   - Compress logs before storing

5. **Right-size Resources**:
```bash
# Analyze actual usage
kubectl top nodes
kubectl top pods --all-namespaces
```

## 🔧 Troubleshooting

### Common Issues

#### 1. Pods Not Starting

```bash
# Check pod status
kubectl get pods -n <namespace>

# Describe pod
kubectl describe pod <pod-name> -n <namespace>

# Check logs
kubectl logs <pod-name> -n <namespace>

# Check events
kubectl get events -n <namespace> --sort-by='.lastTimestamp'
```

#### 2. ArgoCD Application Not Syncing

```bash
# Check application status
argocd app get <app-name>

# View detailed sync status
argocd app sync <app-name> --dry-run

# Force refresh
argocd app get <app-name> --refresh

# Hard refresh
argocd app get <app-name> --hard-refresh
```

#### 3. Ingress Not Working

```bash
# Check ingress
kubectl get ingress -A

# Check ingress controller
kubectl logs -n istio-ingress -l app=istio-ingressgateway

# Check LoadBalancer IP
kubectl get svc -n istio-ingress

# Test DNS resolution
nslookup api.yourdomain.com
```

#### 4. Certificate Issues

```bash
# Check certificates
kubectl get certificate -A

# Check cert-manager logs
kubectl logs -n cert-manager -l app=cert-manager

# Describe certificate
kubectl describe certificate <cert-name> -n <namespace>

# Check challenges
kubectl get challenges -A
```

#### 5. Database Connection Issues

```bash
# Test PostgreSQL connection
kubectl run -it --rm psql-test --image=postgres:15 --restart=Never -- \
  psql -h postgresql-ha-pgpool.databases.svc.cluster.local -U postgres

# Test RabbitMQ connection
kubectl run -it --rm rabbitmq-test --image=rabbitmq:3-management --restart=Never -- \
  rabbitmqctl -n rabbit@rabbitmq-0 status

# Test Redis connection
kubectl run -it --rm redis-test --image=redis:7 --restart=Never -- \
  redis-cli -h redis-cluster-master.middleware.svc.cluster.local ping
```

### Debug Mode

Enable debug logging:

```bash
# ArgoCD
kubectl edit configmap argocd-cmd-params-cm -n argocd
# Add: server.log.level: debug

# Istio
istioctl install --set profile=default \
  --set values.global.logging.level=debug

# Cert-manager
kubectl edit deployment cert-manager -n cert-manager
# Add: --v=4
```

## 🔄 Maintenance

### Upgrade AKS

```bash
# Check available versions
az aks get-upgrades \
  --resource-group rg-aks-prod-eastus \
  --name aks-prod-eastus

# Upgrade cluster
az aks upgrade \
  --resource-group rg-aks-prod-eastus \
  --name aks-prod-eastus \
  --kubernetes-version 1.29.0
```

### Upgrade Components

```bash
# Upgrade ArgoCD
kubectl apply -n argocd -f \
  https://raw.githubusercontent.com/argoproj/argo-cd/v2.10.0/manifests/install.yaml

# Upgrade Istio
istioctl upgrade --set revision=1.20.2

# Upgrade Helm releases
helm repo update
helm upgrade prometheus-stack prometheus-community/kube-prometheus-stack \
  -n observability -f helm-charts/observability/kube-prometheus-stack/values.yaml
```

### Backup

**PostgreSQL Backup:**
```bash
# Create backup job
kubectl create job --from=cronjob/postgresql-backup manual-backup -n databases

# Verify backup
kubectl logs -n databases job/manual-backup
```

**etcd Backup (via AKS):**
```bash
# AKS automatically backs up etcd every 4 hours
# Retention: 7 days
# Restore via Azure Support
```

**Restore from Backup:**
```bash
# Restore PostgreSQL
kubectl exec -it postgresql-0 -n databases -- \
  pg_restore -U postgres -d mydb /backup/dump.sql

# Restore RabbitMQ definitions
kubectl exec -it rabbitmq-0 -n middleware -- \
  rabbitmqctl import_definitions /backup/definitions.json
```

### Disaster Recovery

1. **Infrastructure Recovery:**
```bash
cd terraform/environments/production
terraform plan
terraform apply
```

2. **Application Recovery:**
```bash
# Restore ArgoCD
kubectl apply -f argocd/bootstrap/root-app.yaml

# ArgoCD will sync all applications
argocd app sync --all
```

3. **Data Recovery:**
```bash
# Restore databases from Azure Backup
# Restore persistent volumes from snapshots
```

## 🤝 Contributing

### Development Workflow

1. Fork the repository
2. Create feature branch: `git checkout -b feature/my-feature`
3. Make changes
4. Run validation:
```bash
# Terraform
terraform fmt -recursive
terraform validate

# Helm
helm lint helm-charts/*/

# YAML
yamllint .
```
5. Commit: `git commit -am 'Add feature'`
6. Push: `git push origin feature/my-feature`
7. Create Pull Request

### Code Standards

- Use consistent naming conventions
- Document all Terraform variables
- Include examples in Helm values files
- Write clear commit messages
- Update documentation with changes

### Testing

```bash
# Terraform
terraform plan -out=tfplan

# Helm
helm template helm-charts/observability/kube-prometheus-stack/ | kubectl apply --dry-run=client -f -

# ArgoCD
argocd app diff <app-name>
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/your-org/azure-aks-infrastructure/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-org/azure-aks-infrastructure/discussions)
- **Documentation**: [docs/](docs/)

## 🙏 Acknowledgments

- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest)
- [ArgoCD](https://argoproj.github.io/cd/)
- [Istio](https://istio.io/)
- [Kong](https://konghq.com/)
- [Prometheus Operator](https://github.com/prometheus-operator/prometheus-operator)
- [Bitnami Helm Charts](https://github.com/bitnami/charts)

## 📚 Additional Resources

- [Azure AKS Best Practices](https://learn.microsoft.com/en-us/azure/aks/best-practices)
- [Kubernetes Production Best Practices](https://learnk8s.io/production-best-practices)
- [Istio Documentation](https://istio.io/latest/docs/)
- [ArgoCD Best Practices](https://argo-cd.readthedocs.io/en/stable/user-guide/best_practices/)
- [Terraform Best Practices](https://www.terraform-best-practices.com/)

---

**Made with ❤️ for Production-Ready Infrastructure**

*Last Updated: November 2025*