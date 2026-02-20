# 📋 AI-ATC Repository Index

This document provides a comprehensive index of the AI-ATC (Air Traffic Control) repository, documenting all components, their relationships, and functionality.

## 🏗️ Project Architecture

### Overview
AI-ATC is a sophisticated air traffic control simulation system that combines:
- **Reinforcement Learning** for AI agent training
- **Microservices Architecture** for scalability
- **Real-time Simulation** with physics-based aircraft dynamics
- **Voice Recognition** for natural ATC communication
- **Web-based Interface** for user interaction

### Technology Stack
- **Backend**: C# .NET 10.0 (Microservices)
- **Frontend**: Blazor WebAssembly
- **AI/ML**: Python, TensorFlow, Stable-Baselines3, Gymnasium
- **Database**: PostgreSQL with Entity Framework Core
- **Infrastructure**: Docker, Kubernetes, Azure, Terraform
- **Monitoring**: Prometheus, Grafana
- **CI/CD**: GitHub Actions

## 📁 Directory Structure

### Root Level Files
```
AI-ATC/
├── 🐍 Python ML Components
│   ├── ai_atc_env.py          # Custom Gymnasium environment for RL training
│   ├── airplane.py           # Aircraft physics and dynamics model
│   ├── airport.py            # Airport and runway definitions
│   ├── runway.py             # Runway configuration and approach paths
│   ├── train_ai_atc.py       # PPO training script for AI agent
│   ├── visualize_ai_atc.py   # Training visualization and animation
│   ├── evaluate_model.py     # Model evaluation and testing
│   ├── constants.py          # Simulation constants and configuration
│   ├── conversion.py         # Unit conversion utilities
│   ├── curriculum.py         # Curriculum learning implementation
│   ├── session_manager.py    # Training session management
│   ├── vfr_support.py        # VFR flight support
│   ├── requirements.txt      # Python dependencies
│   └── Dockerfile            # GPU-enabled Python development container
│
├── 🌐 Web Application (src/AIATC.Web/)
│   ├── Program.cs            # Blazor application entry point
│   ├── App.razor            # Main application component
│   ├── Pages/               # Application pages
│   │   ├── Simulation.razor # Main simulation interface
│   │   └── ChallengeMode.razor # Challenge mode interface
│   ├── Components/          # Reusable UI components
│   │   ├── Radar/           # Radar display components
│   │   ├── Audio/           # Voice command interface
│   │   └── Controls/        # ATC control panels
│   └── Services/            # Client-side services
│
├── 🏗️ Microservices (src/)
│   ├── AIATC.AIAgentService/    # AI agent orchestration service
│   ├── AIATC.AudioService/      # Voice recognition and synthesis
│   ├── AIATC.SimulationService/ # Core simulation engine
│   ├── AIATC.ScenarioService/   # Scenario management and generation
│   ├── AIATC.WorldDataService/  # World data and airport information
│   ├── AIATC.UserService/       # User management and authentication
│   ├── AIATC.Data/              # Data access layer
│   ├── AIATC.Domain/            # Business logic and models
│   └── AIATC.Common/            # Shared utilities and models
│
├── 📊 Data and Models
│   ├── models/              # Trained AI models
│   ├── scenarios/           # Predefined scenarios
│   ├── live_data/           # Real-time data integration
│   └── publish/             # Published application artifacts
│
├── 🚀 Deployment & Infrastructure
│   ├── terraform/           # Infrastructure as Code
│   │   └── azure/           # Azure-specific configurations
│   ├── helm/                # Kubernetes deployment charts
│   │   └── aiatc/           # AIATC application chart
│   ├── docker-compose.yml   # Local development environment
│   ├── docker-compose.monitoring.yml # Monitoring stack
│   └── prometheus.yml       # Prometheus monitoring configuration
│
├── 📚 Documentation (docs/)
│   ├── GETTING_STARTED.md   # Installation and setup guide
│   ├── ATC_COMMAND_REFERENCE.md # ATC command documentation
│   ├── SCENARIOS.md         # Scenario descriptions
│   ├── SCORING_SYSTEM.md    # Scoring and evaluation
│   ├── AZURE_SPEECH_INTEGRATION.md # Azure Speech Services integration
│   ├── MICROSERVICES_ARCHITECTURE.md # Architecture documentation
│   ├── DATABASE_SCHEMA.md   # Database structure
│   ├── PERFORMANCE_OPTIMIZATION.md # Performance tuning
│   ├── SECURITY_AUDIT.md    # Security considerations
│   └── TROUBLESHOOTING.md   # Common issues and solutions
│
├── 🧪 Testing
│   ├── tests/               # Unit and integration tests
│   │   ├── AIATC.Domain.Tests/
│   │   ├── AIATC.SimulationService.Tests/
│   │   └── integration/     # Integration test suites
│   └── k6/                  # Load testing scripts
│
└── 📈 Monitoring & Performance
    ├── performance/         # Performance profiling tools
    ├── security/            # Security scanning scripts
    └── tensorboard/         # ML training visualization
```

## 🔧 Core Components

### 1. AI Agent Service (`src/AIATC.AIAgentService/`)
**Purpose**: Orchestrates AI agent training and inference
- **TensorFlowModelService**: Handles model loading and prediction
- **AIAgentService**: Coordinates between simulation and AI components
- **Protos/**: gRPC service definitions for inter-service communication

### 2. Audio Service (`src/AIATC.AudioService/`)
**Purpose**: Manages voice recognition and synthesis
- **Speech Recognition**: Converts pilot voice commands to text
- **Text-to-Speech**: Converts ATC instructions to audio
- **Audio Processing**: Handles audio input/output streams

### 3. Simulation Service (`src/AIATC.SimulationService/`)
**Purpose**: Core simulation engine with physics-based aircraft dynamics
- **Aircraft Physics**: Realistic flight dynamics and control laws
- **Environment Simulation**: Weather, airspace, and traffic simulation
- **Real-time Updates**: High-frequency simulation updates

### 4. Scenario Service (`src/AIATC.ScenarioService/`)
**Purpose**: Manages training scenarios and difficulty progression
- **Scenario Builder**: Creates complex multi-aircraft scenarios
- **Difficulty Scaling**: Progressive challenge increases
- **Scenario Persistence**: Saves and loads scenario configurations

### 5. World Data Service (`src/AIATC.WorldDataService/`)
**Purpose**: Provides geographical and aviation data
- **Airport Data**: Runway configurations, approach plates
- **Airspace Data**: Airspace boundaries and restrictions
- **Navigation Aids**: VOR, NDB, and ILS information

### 6. User Service (`src/AIATC.UserService/`)
**Purpose**: User management and authentication
- **User Profiles**: Pilot and ATC user accounts
- **Authentication**: Secure login and session management
- **Preferences**: User-specific settings and configurations

### 7. Web Application (`src/AIATC.Web/`)
**Purpose**: User interface for simulation and training
- **Radar Display**: Real-time aircraft tracking visualization
- **ATC Interface**: Command input and communication panels
- **Training Dashboard**: Progress tracking and performance metrics

## 🤖 Machine Learning Components

### Reinforcement Learning Environment (`ai_atc_env.py`)
**Framework**: Gymnasium + Stable-Baselines3
**Algorithm**: Proximal Policy Optimization (PPO)
**Features**:
- Hierarchical control architecture
- Curriculum learning across flight phases
- Dense shaping rewards for sparse objectives
- Multi-aircraft collision avoidance
- Physics-based aircraft dynamics

### Aircraft Dynamics (`airplane.py`)
**Model**: 6-DOF physics simulation
**Features**:
- Realistic control laws and envelope protection
- Aerodynamic forces and moments
- Engine performance modeling
- Environmental effects (wind, turbulence)

### Training Pipeline (`train_ai_atc.py`)
**Components**:
- Environment initialization
- PPO agent configuration
- Curriculum progression
- Model checkpointing
- Performance monitoring

## 🗄️ Data Architecture

### Database Schema (`src/AIATC.Data/`)
**Database**: PostgreSQL with Entity Framework Core
**Entities**:
- **Users**: Pilot and ATC user accounts
- **Aircraft**: Aircraft types and performance data
- **Airports**: Airport and runway information
- **Scenarios**: Training scenario configurations
- **Sessions**: Training session logs and metrics
- **Performance**: User performance tracking

### Data Seeding (`src/AIATC.Data/Seeding/`)
**Purpose**: Initial data population
- **Aviation Data**: Real-world airport and aircraft data
- **User Accounts**: Default administrative accounts
- **Scenarios**: Predefined training scenarios

## 🚀 Deployment Architecture

### Infrastructure as Code (`terraform/azure/`)
**Components**:
- **Azure Kubernetes Service (AKS)**: Container orchestration
- **Azure Container Registry**: Docker image storage
- **Azure PostgreSQL**: Database hosting
- **Azure Redis**: Caching layer
- **Azure Storage**: File and blob storage

### Container Orchestration (`helm/aiatc/`)
**Charts**:
- **Web Application**: Blazor frontend deployment
- **Microservices**: Individual service deployments
- **Database**: PostgreSQL deployment with migrations
- **Monitoring**: Prometheus and Grafana stack

### Development Environment (`docker-compose.yml`)
**Services**:
- **Web Application**: Blazor frontend
- **API Services**: All microservices
- **Database**: PostgreSQL with sample data
- **Redis**: Caching service
- **Monitoring**: Prometheus and Grafana

## 📊 Monitoring & Observability

### Prometheus Configuration (`prometheus.yml`)
**Metrics**:
- Application performance metrics
- Service health checks
- Resource utilization
- Custom business metrics

### Performance Profiling (`performance/`)
**Tools**:
- **Performance Profiler**: Application performance analysis
- **Load Testing**: K6-based stress testing
- **Memory Analysis**: Memory usage optimization

## 🛡️ Security & Compliance

### Security Scanning (`security/`)
**Components**:
- **Container Security**: Docker image vulnerability scanning
- **Secrets Management**: Secure credential handling
- **Security Middleware**: Application security layers

### Security Audit (`docs/SECURITY_AUDIT.md`)
**Areas**:
- Authentication and authorization
- Data encryption and protection
- Network security
- Compliance requirements

## 📈 Development Workflow

### CI/CD Pipeline (`.github/workflows/`)
**Stages**:
1. **Code Quality**: Linting and static analysis
2. **Testing**: Unit, integration, and load tests
3. **Build**: Docker image creation and registry push
4. **Deploy**: Staging and production deployments

### Development Tools (`.devcontainer/`)
**Features**:
- VS Code development container
- Pre-configured development environment
- Docker-in-Docker support
- Integrated debugging

## 🎮 User Experience

### Simulation Interface
- **Radar Display**: Real-time aircraft tracking
- **ATC Console**: Command input and communication
- **Voice Commands**: Natural language interaction
- **Training Modes**: Progressive difficulty levels

### Challenge System
- **Scenarios**: Predefined challenging situations
- **Scoring**: Performance-based evaluation
- **Leaderboards**: Competitive ranking system
- **Achievements**: Milestone-based rewards

## 🔗 Integration Points

### External APIs
- **Azure Speech Services**: Voice recognition and synthesis
- **ADSB Exchange**: Real-time flight data
- **Weather APIs**: Real-time weather conditions
- **Navigation Data**: Aeronautical charts and procedures

### Communication Protocols
- **gRPC**: Inter-service communication
- **HTTP/REST**: External API integration
- **WebSocket**: Real-time client updates
- **SignalR**: Real-time communication

## 📋 File Index

### Source Code Files
| File | Purpose | Size | Last Modified |
|------|---------|------|---------------|
| `ai_atc_env.py` | RL environment implementation | 15KB | 2026-02-14 |
| `airplane.py` | Aircraft physics model | 25KB | 2026-02-14 |
| `train_ai_atc.py` | PPO training script | 8KB | 2026-02-14 |
| `src/AIATC.Web/Program.cs` | Blazor application entry | 2KB | 2026-02-14 |
| `src/AIATC.Domain/Models/User.cs` | User entity model | 4KB | 2026-02-14 |
| `terraform/azure/main.tf` | Azure infrastructure | 6KB | 2026-02-14 |

### Configuration Files
| File | Purpose | Environment |
|------|---------|-------------|
| `appsettings.json` | Application configuration | All services |
| `docker-compose.yml` | Local development setup | Development |
| `helm/aiatc/values.yaml` | Kubernetes configuration | Production |
| `terraform/azure/terraform.tfvars` | Azure deployment variables | Production |

### Documentation Files
| File | Purpose | Status |
|------|---------|--------|
| `README.md` | Project overview | Complete |
| `docs/GETTING_STARTED.md` | Installation guide | Complete |
| `docs/ATC_COMMAND_REFERENCE.md` | Command documentation | Complete |
| `docs/MICROSERVICES_ARCHITECTURE.md` | Architecture guide | Complete |

## 🔄 Development Status

### Completed Features ✅
- [x] Core simulation engine with physics
- [x] Reinforcement learning environment
- [x] Web-based user interface
- [x] Microservices architecture
- [x] Azure deployment infrastructure
- [x] Voice recognition integration
- [x] Multi-aircraft scenarios
- [x] Performance monitoring

### In Progress 🔄
- [ ] Advanced weather simulation
- [ ] Real-time flight data integration
- [ ] Mobile application interface
- [ ] Multi-language support

### Planned Features 📋
- [ ] VR/AR interface support
- [ ] Advanced ATC procedures
- [ ] Multi-user collaborative training
- [ ] Machine learning model optimization

## 🎯 Usage Examples

### Training an AI Agent
```bash
# Start the training environment
python train_ai_atc.py --curriculum-stage 2 --episodes 1000

# Monitor training progress
python visualize_ai_atc.py --model-path models/ppo_model.zip
```

### Running the Simulation
```bash
# Start local development environment
docker-compose up -d

# Access web interface
open http://localhost:5000
```

### Deploying to Azure
```bash
# Initialize infrastructure
terraform init
terraform apply -var-file="terraform/azure/terraform.tfvars"

# Deploy application
helm install aiatc helm/aiatc/
```

## 📞 Support & Contributing

### Getting Help
- **Documentation**: See `docs/` directory for comprehensive guides
- **Issues**: Report bugs and feature requests on GitHub
- **Discussions**: Join community discussions for support

### Contributing
1. Fork the repository
2. Create a feature branch
3. Make changes with tests
4. Submit a pull request
5. Follow code review guidelines

### Code Style
- C#/.NET: Follow Microsoft .NET coding guidelines
- Python: PEP 8 compliance
- Documentation: Markdown with consistent formatting
- Git: Conventional commit messages

---

*This index was generated on 2026-02-14 and provides a comprehensive overview of the AI-ATC repository structure and components.*