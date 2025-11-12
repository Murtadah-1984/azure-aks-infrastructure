# GitHub Actions Workflows

This directory contains GitHub Actions workflows for CI/CD of the Azure AKS infrastructure repository.

## Workflows Overview

### 1. **CI Pipeline** (`ci.yml`)
Main CI pipeline that orchestrates all validation and linting jobs.

**Triggers:**
- Pull requests to `main` or `develop`
- Pushes to `main` or `develop`
- Manual dispatch

**Jobs:**
- Lint and Validate (YAML, Shell scripts)
- Terraform Validate (via reusable workflow)
- Helm Lint (via reusable workflow)
- Security Scan (via reusable workflow)
- Build Status (aggregates results)

### 2. **Terraform Plan** (`terraform-plan.yml`)
Runs Terraform plan to preview infrastructure changes.

**Triggers:**
- Pull requests affecting `terraform/**`
- Manual dispatch with environment selection

**Features:**
- Validates Terraform configuration
- Checks formatting
- Creates plan and comments on PR
- Uploads plan artifact for apply workflow

**Required Secrets:**
- `AZURE_CLIENT_ID`
- `AZURE_CLIENT_SECRET`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`
- `TF_STATE_RG` (Terraform state resource group)
- `TF_STATE_SA` (Terraform state storage account)
- `TF_STATE_CONTAINER` (Terraform state container)

### 3. **Terraform Apply** (`terraform-apply.yml`)
Applies Terraform changes to infrastructure.

**Triggers:**
- Pushes to `main` or `develop` affecting `terraform/**`
- Manual dispatch with environment selection

**Features:**
- Downloads plan artifact from plan workflow
- Applies infrastructure changes
- Retrieves AKS credentials after apply
- Environment protection (requires approval for production)

**Required Secrets:**
- Same as Terraform Plan
- Environment-specific secrets configured in GitHub Environments

### 4. **Terraform Validate** (`terraform-validate.yml`)
Validates Terraform code quality and formatting.

**Triggers:**
- Pull requests affecting `terraform/**`
- Pushes to `main` or `develop`
- Manual dispatch
- Called by CI pipeline

**Features:**
- Format checking and auto-formatting
- Terraform validation
- TFLint for best practices
- Sensitive data detection

### 5. **Helm Lint** (`helm-lint.yml`)
Validates Helm charts and ArgoCD applications.

**Triggers:**
- Pull requests affecting `helm-charts/**` or `argocd/**`
- Pushes to `main` or `develop`
- Manual dispatch
- Called by CI pipeline

**Features:**
- Helm chart linting
- Kubernetes manifest validation
- Template rendering tests
- ArgoCD application validation

### 6. **Security Scan** (`security-scan.yml`)
Scans for security vulnerabilities and secrets.

**Triggers:**
- Pull requests
- Pushes to `main` or `develop`
- Weekly schedule (Sundays)
- Manual dispatch
- Called by CI pipeline

**Features:**
- Secret scanning with Gitleaks
- Dependency vulnerability scanning with Trivy
- Terraform security scanning with Checkov and TFSec
- SARIF upload to GitHub Security

### 7. **ArgoCD Sync** (`argocd-sync.yml`)
Synchronizes ArgoCD applications.

**Triggers:**
- Manual dispatch (with options)
- Pushes to `main` affecting `argocd/**`, `helm-charts/**`, or `manifests/**`

**Features:**
- Sync individual or all applications
- Hard refresh option
- Prune option
- Application status reporting

**Required Secrets:**
- `AZURE_CLIENT_ID`
- `AZURE_CLIENT_SECRET`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`
- `AKS_RESOURCE_GROUP`
- `AKS_CLUSTER_NAME`
- `ARGOCD_ADMIN_PASSWORD`

## Setup Instructions

### 1. Configure GitHub Secrets

Go to **Settings > Secrets and variables > Actions** and add:

**Azure Authentication:**
```
AZURE_CLIENT_ID
AZURE_CLIENT_SECRET
AZURE_TENANT_ID
AZURE_SUBSCRIPTION_ID
```

**Terraform State:**
```
TF_STATE_RG
TF_STATE_SA
TF_STATE_CONTAINER
```

**ArgoCD:**
```
ARGOCD_ADMIN_PASSWORD
AKS_RESOURCE_GROUP
AKS_CLUSTER_NAME
```

### 2. Configure GitHub Environments

Create environments for `dev`, `staging`, and `production`:

1. Go to **Settings > Environments**
2. Create each environment
3. Add environment-specific secrets
4. Configure protection rules (required reviewers for production)

### 3. Enable Dependabot

Dependabot is configured via `.github/dependabot.yml` to:
- Update GitHub Actions weekly
- Update Terraform providers weekly
- Create PRs with appropriate labels

### 4. Configure Branch Protection

Recommended branch protection rules:

**For `main` branch:**
- Require pull request reviews (1 approval)
- Require status checks to pass:
  - `terraform-validate`
  - `helm-lint`
  - `security-scan`
- Require branches to be up to date
- Do not allow force pushes

**For `develop` branch:**
- Require pull request reviews (1 approval)
- Require status checks to pass
- Allow force pushes (for hotfixes)

## Workflow Best Practices

### Security
- ✅ All actions pinned to full SHA or major version
- ✅ Secrets stored in GitHub Secrets
- ✅ Minimal permissions (principle of least privilege)
- ✅ OIDC for Azure authentication (recommended)
- ✅ No hardcoded secrets

### Performance
- ✅ Path filters to avoid unnecessary runs
- ✅ Caching where applicable
- ✅ Parallel job execution
- ✅ Concurrency limits

### Maintainability
- ✅ Reusable workflows for common tasks
- ✅ Clear workflow names and descriptions
- ✅ Proper error handling
- ✅ Artifact retention policies

## Troubleshooting

### Terraform Plan Fails
1. Check Azure authentication credentials
2. Verify Terraform state backend configuration
3. Review Terraform syntax errors in logs

### Terraform Apply Requires Approval
- Production environment requires manual approval
- Go to Actions tab and approve the workflow run

### Security Scan Finds False Positives
- Update `.gitleaks.toml` allowlist
- Review and add patterns to exclude

### ArgoCD Sync Fails
1. Verify AKS cluster is accessible
2. Check ArgoCD server is running
3. Verify ArgoCD admin password is correct

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest)
- [ArgoCD Documentation](https://argo-cd.readthedocs.io/)
- [Helm Documentation](https://helm.sh/docs/)

