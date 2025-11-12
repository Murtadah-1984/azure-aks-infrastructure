#!/bin/bash

cat << 'EOF'
╔══════════════════════════════════════════════════════════════╗
║          API Gateway Decision Helper                         ║
╚══════════════════════════════════════════════════════════════╝

Choose your API Gateway based on your requirements:

┌─────────────────────────────────────────────────────────────┐
│ ISTIO SERVICE MESH                                          │
├─────────────────────────────────────────────────────────────┤
│ Best for:                                                   │
│ • Microservices architecture (10+ services)                 │
│ • Need service-to-service mTLS                              │
│ • Advanced traffic management (canary, A/B testing)         │
│ • Deep observability (distributed tracing)                  │
│ • Service mesh features (circuit breakers, retries)        │
│                                                             │
│ Pros:                                                       │
│ ✓ Zero-trust security model                                │
│ ✓ Automatic service discovery                              │
│ ✓ Rich telemetry and observability                         │
│ ✓ Advanced traffic management                              │
│ ✓ Multi-cluster support                                    │
│                                                             │
│ Cons:                                                       │
│ ✗ Steeper learning curve                                   │
│ ✗ Higher resource overhead                                 │
│ ✗ More complex troubleshooting                             │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ KONG API GATEWAY                                            │
├─────────────────────────────────────────────────────────────┤
│ Best for:                                                   │
│ • Traditional API gateway pattern                           │
│ • External API management                                   │
│ • Need developer portal                                     │
│ • Plugin ecosystem important                                │
│ • Simpler deployment model                                  │
│                                                             │
│ Pros:                                                       │
│ ✓ Easier to understand and deploy                          │
│ ✓ Rich plugin ecosystem                                    │
│ ✓ Great admin UI                                           │
│ ✓ Lower resource footprint                                 │
│ ✓ Excellent documentation                                  │
│                                                             │
│ Cons:                                                       │
│ ✗ Less service mesh features                               │
│ ✗ No automatic mTLS between services                       │
│ ✗ Limited multi-cluster support                            │
└─────────────────────────────────────────────────────────────┘

Resource Requirements:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Istio:
  Control Plane: 2 vCPU, 4GB RAM
  Per Service Sidecar: 100m CPU, 128Mi RAM
  Gateway: 500m CPU, 512Mi RAM (×3 replicas)
  
Kong:
  Gateway: 500m CPU, 1GB RAM (×3 replicas)
  Database: Shared PostgreSQL HA cluster
  Controller: 100m CPU, 256Mi RAM

Recommendation for your setup:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Based on your infrastructure (observability, RabbitMQ, Redis, SQL):

👉 ISTIO if you have:
   - Multiple microservices communicating internally
   - Need mTLS between services
   - Want distributed tracing across services
   - Planning service mesh features

👉 KONG if you have:
   - Primarily external-facing APIs
   - Simpler architecture
   - Want quick deployment
   - Need extensive plugin support

💡 You can also deploy BOTH:
   - Istio for internal service mesh
   - Kong for external API gateway
   - Use Istio's ingress gateway for internal traffic
   - Use Kong for public APIs

EOF

read -p "Which gateway do you want to deploy? (istio/kong/both): " choice

case $choice in
  istio)
    echo "Deploying Istio Service Mesh..."
    kubectl apply -f ../argocd/applications/infrastructure/istio-base.yaml
    kubectl apply -f ../argocd/applications/infrastructure/istio-istiod.yaml
    kubectl apply -f ../argocd/applications/infrastructure/istio-gateway.yaml
    ;;
  kong)
    echo "Deploying Kong API Gateway..."
    kubectl apply -f ../argocd/applications/infrastructure/kong-gateway.yaml
    ;;
  both)
    echo "Deploying both Istio and Kong..."
    kubectl apply -f ../argocd/applications/infrastructure/istio-base.yaml
    kubectl apply -f ../argocd/applications/infrastructure/istio-istiod.yaml
    kubectl apply -f ../argocd/applications/infrastructure/istio-gateway.yaml
    kubectl apply -f ../argocd/applications/infrastructure/kong-gateway.yaml
    ;;
  *)
    echo "Invalid choice"
    exit 1
    ;;
esac