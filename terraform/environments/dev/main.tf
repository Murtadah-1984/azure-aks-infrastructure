terraform {
  required_version = ">= 1.5"
  
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
  }
  
  backend "azurerm" {
    resource_group_name  = "terraform-state-rg"
    storage_account_name = "tfstatexxxxx"
    container_name       = "tfstate"
    key                  = "dev/aks.tfstate"
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy = false
    }
  }
}

module "networking" {
  source = "../../modules/networking"
  
  resource_group_name = var.resource_group_name
  location            = var.location
  vnet_address_space  = ["10.1.0.0/16"]
  
  subnets = {
    aks = {
      address_prefixes = ["10.1.0.0/20"]
    }
    data = {
      address_prefixes = ["10.1.16.0/24"]
    }
  }
}

module "aks" {
  source = "../../modules/aks"
  
  cluster_name        = "aks-${var.environment}-${var.location}"
  location            = var.location
  resource_group_name = var.resource_group_name
  kubernetes_version  = var.kubernetes_version
  
  subnet_id = module.networking.subnet_ids["aks"]
  
  system_node_count     = 3
  system_node_size      = "Standard_D4s_v5"
  system_node_min_count = 3
  system_node_max_count = 6
  
  workload_node_count     = 3
  workload_node_size      = "Standard_D8s_v5"
  workload_node_min_count = 3
  workload_node_max_count = 10
  
  admin_group_object_ids = var.admin_group_object_ids
  
  tags = var.tags
}