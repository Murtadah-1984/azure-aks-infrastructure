#!/bin/bash
set -e

NAMESPACE="argocd"
ARGOCD_VERSION="v2.9.3"

echo "Installing ArgoCD..."
kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -

kubectl apply -n $NAMESPACE -f \
  https://raw.githubusercontent.com/argoproj/argo-cd/$ARGOCD_VERSION/manifests/install.yaml

echo "Waiting for ArgoCD to be ready..."
kubectl wait --for=condition=available --timeout=600s \
  deployment/argocd-server -n $NAMESPACE

# Get initial admin password
ARGOCD_PASSWORD=$(kubectl -n $NAMESPACE get secret argocd-initial-admin-secret \
  -o jsonpath="{.data.password}" | base64 -d)

echo "ArgoCD Admin Password: $ARGOCD_PASSWORD"

# Deploy root application
kubectl apply -f ../argocd/bootstrap/root-app.yaml

echo "ArgoCD deployed successfully!"
echo "Access ArgoCD UI: kubectl port-forward svc/argocd-server -n argocd 8080:443"