"""
Enhanced curriculum learning for AI-ATC agent.
Implements progressive difficulty stages with adaptive transitions and performance tracking.
"""

import numpy as np
from typing import Dict, List, Optional, Tuple, TYPE_CHECKING
from dataclasses import dataclass, field

if TYPE_CHECKING:
    from stable_baselines3 import PPO
    from stable_baselines3.common.vec_env import VecEnv


@dataclass
class CurriculumStage:
    """Configuration for a single curriculum stage."""
    stage: int
    name: str
    description: str
    timesteps: int
    target_reward: Optional[float] = None
    min_performance_threshold: float = 0.5  # % of target to proceed
    max_timesteps_per_episode: int = 2000

    # Environment difficulty parameters
    num_planes: int = 4
    initial_distance_nm: float = 10.0
    max_distance_nm: float = 30.0
    min_altitude_ft: float = 2000.0
    max_altitude_ft: float = 8000.0
    intercept_angle_range: Tuple[float, float] = (0.0, 0.0)
    separation_distance: float = 3.0

    # Reward shaping
    landing_reward: float = 100.0
    collision_penalty: float = 200.0
    efficiency_bonus: float = 0.0
    time_efficiency_weight: float = 0.0


@dataclass
class CurriculumMetrics:
    """Tracks metrics for curriculum progression."""
    stage: int
    episode_count: int = 0
    total_timesteps: int = 0
    mean_reward: float = 0.0
    max_reward: float = float('-inf')
    min_reward: float = float('inf')
    landing_success_rate: float = 0.0
    collision_rate: float = 0.0
    rewards_history: List[float] = field(default_factory=list)

    def update(self, episode_reward: float):
        """Update metrics with new episode result."""
        self.episode_count += 1
        self.rewards_history.append(episode_reward)
        self.mean_reward = np.mean(self.rewards_history[-100:])  # Last 100 episodes
        self.max_reward = max(self.max_reward, episode_reward)
        self.min_reward = min(self.min_reward, episode_reward)

    def convergence_percentage(self, target_reward: Optional[float]) -> float:
        """Calculate what percentage of target reward we've achieved."""
        if target_reward is None or target_reward == 0:
            return 100.0
        return max(0.0, min(100.0, (self.mean_reward / target_reward) * 100))


class AdaptiveCurriculum:
    """Adaptive curriculum learning with dynamic stage progression."""

    def __init__(self):
        self.stages: List[CurriculumStage] = self._create_default_stages()
        self.current_stage_idx = 0
        self.metrics: Dict[int, CurriculumMetrics] = {}
        self.stage_history: List[Tuple[int, float, float]] = []  # (stage, mean_reward, timestamp)

    def _create_default_stages(self) -> List[CurriculumStage]:
        """Create default curriculum stages with progressive difficulty."""
        return [
            CurriculumStage(
                stage=0,
                name="Basic Single Approach",
                description="Single aircraft on final approach, on localizer",
                timesteps=200_000,
                target_reward=500.0,
                num_planes=1,
                initial_distance_nm=10.0,
                max_distance_nm=10.0,
                min_altitude_ft=2000.0,
                max_altitude_ft=2000.0,
                intercept_angle_range=(0.0, 0.0),
                landing_reward=100.0,
            ),
            CurriculumStage(
                stage=1,
                name="Single with Intercept",
                description="Single aircraft with 20-30 degree intercept angle",
                timesteps=250_000,
                target_reward=600.0,
                num_planes=1,
                initial_distance_nm=15.0,
                max_distance_nm=15.0,
                min_altitude_ft=4000.0,
                max_altitude_ft=4000.0,
                intercept_angle_range=(-30.0, 30.0),
                landing_reward=120.0,
            ),
            CurriculumStage(
                stage=2,
                name="Dual Arrivals",
                description="Two aircraft approaching from different angles",
                timesteps=300_000,
                target_reward=700.0,
                num_planes=2,
                initial_distance_nm=12.0,
                max_distance_nm=20.0,
                min_altitude_ft=3000.0,
                max_altitude_ft=6000.0,
                intercept_angle_range=(-20.0, 20.0),
                landing_reward=110.0,
                collision_penalty=250.0,
            ),
            CurriculumStage(
                stage=3,
                name="Multiple Approaches",
                description="Three to four aircraft with varied entry points",
                timesteps=400_000,
                target_reward=900.0,
                num_planes=4,
                initial_distance_nm=10.0,
                max_distance_nm=25.0,
                min_altitude_ft=2000.0,
                max_altitude_ft=10000.0,
                intercept_angle_range=(-45.0, 45.0),
                landing_reward=100.0,
                collision_penalty=300.0,
                efficiency_bonus=1.0,
            ),
            CurriculumStage(
                stage=4,
                name="Complex Terminal Area",
                description="Full terminal area with realistic arrivals and conflicts",
                timesteps=500_000,
                target_reward=1200.0,
                num_planes=6,
                initial_distance_nm=15.0,
                max_distance_nm=30.0,
                min_altitude_ft=1000.0,
                max_altitude_ft=12000.0,
                intercept_angle_range=(-60.0, 60.0),
                landing_reward=100.0,
                collision_penalty=400.0,
                efficiency_bonus=2.0,
                time_efficiency_weight=0.1,
            ),
            CurriculumStage(
                stage=5,
                name="Expert Scenario",
                description="Challenging scenarios with maximum complexity",
                timesteps=600_000,
                target_reward=1500.0,
                num_planes=8,
                initial_distance_nm=20.0,
                max_distance_nm=40.0,
                min_altitude_ft=500.0,
                max_altitude_ft=15000.0,
                intercept_angle_range=(-90.0, 90.0),
                landing_reward=100.0,
                collision_penalty=500.0,
                efficiency_bonus=3.0,
                time_efficiency_weight=0.2,
            ),
        ]

    @property
    def current_stage(self) -> CurriculumStage:
        """Get current curriculum stage."""
        return self.stages[self.current_stage_idx]

    def get_stage(self, stage_idx: int) -> CurriculumStage:
        """Get a specific curriculum stage."""
        if 0 <= stage_idx < len(self.stages):
            return self.stages[stage_idx]
        return self.stages[-1]

    def initialize_stage_metrics(self):
        """Initialize metrics tracking for current stage."""
        stage = self.current_stage
        self.metrics[stage.stage] = CurriculumMetrics(stage=stage.stage)

    def update_stage_metrics(self, episode_reward: float):
        """Update metrics for current stage."""
        stage = self.current_stage
        if stage.stage not in self.metrics:
            self.initialize_stage_metrics()
        self.metrics[stage.stage].update(episode_reward)

    def should_advance_stage(self) -> bool:
        """Determine if current stage should be advanced."""
        if self.current_stage_idx >= len(self.stages) - 1:
            return False

        stage = self.current_stage
        metrics = self.metrics.get(stage.stage)

        if metrics is None or metrics.episode_count < 10:
            return False

        if stage.target_reward is None:
            return metrics.mean_reward > 500.0  # Default threshold

        threshold = stage.target_reward * (stage.min_performance_threshold / 100.0)
        convergence = metrics.mean_reward >= threshold
        min_episodes = max(50, stage.timesteps // 2000)  # Rough estimate

        return convergence and metrics.episode_count >= min_episodes

    def advance_stage(self) -> bool:
        """Advance to next curriculum stage."""
        if self.should_advance_stage():
            self.current_stage_idx += 1
            self.initialize_stage_metrics()
            return True
        return False

    def get_stage_config_dict(self) -> Dict:
        """Get current stage configuration as dictionary for environment."""
        stage = self.current_stage
        return {
            'stage': stage.stage,
            'num_planes': stage.num_planes,
            'initial_distance_nm': stage.initial_distance_nm,
            'max_distance_nm': stage.max_distance_nm,
            'min_altitude_ft': stage.min_altitude_ft,
            'max_altitude_ft': stage.max_altitude_ft,
            'intercept_angle_range': stage.intercept_angle_range,
            'separation_distance': stage.separation_distance,
            'landing_reward': stage.landing_reward,
            'collision_penalty': stage.collision_penalty,
        }

    def get_summary(self) -> str:
        """Get summary of curriculum progress."""
        stage = self.current_stage
        metrics = self.metrics.get(stage.stage)

        summary = f"\n{'='*60}\n"
        summary += f"Curriculum Stage {stage.stage}: {stage.name}\n"
        summary += f"Description: {stage.description}\n"
        summary += f"{'='*60}\n"

        if metrics:
            summary += f"Episodes: {metrics.episode_count}\n"
            summary += f"Mean Reward: {metrics.mean_reward:.2f}\n"
            summary += f"Max Reward: {metrics.max_reward:.2f}\n"
            summary += f"Min Reward: {metrics.min_reward:.2f}\n"

            if stage.target_reward:
                convergence = metrics.convergence_percentage(stage.target_reward)
                summary += f"Target Reward: {stage.target_reward:.2f}\n"
                summary += f"Convergence: {convergence:.1f}%\n"

        summary += f"{'='*60}\n"
        return summary


def train_with_adaptive_curriculum(
    model,
    env,
    curriculum: Optional[AdaptiveCurriculum] = None,
    total_episodes: int = 2000,
    eval_interval: int = 100,
    verbose: bool = True
):
    """
    Train model with adaptive curriculum learning.

    Args:
        model: PPO model to train
        env: Vectorized environment
        curriculum: Curriculum object (creates default if None)
        total_episodes: Total training episodes
        eval_interval: Evaluate every N episodes
        verbose: Print progress information

    Returns:
        Trained model and curriculum object
    """
    if curriculum is None:
        curriculum = AdaptiveCurriculum()

    curriculum.initialize_stage_metrics()
    episode_count = 0

    while episode_count < total_episodes:
        stage = curriculum.current_stage

        if verbose:
            print(curriculum.get_summary())

        # Train on current stage
        timesteps_this_stage = stage.timesteps

        # Set environment to current stage
        try:
            env.env_method("set_curriculum_stage", curriculum.current_stage_idx)
        except AttributeError:
            if verbose:
                print("Warning: Environment doesn't support set_curriculum_stage")

        # Learn
        model.learn(total_timesteps=timesteps_this_stage, reset_num_timesteps=False)

        # Check if we should advance
        if curriculum.advance_stage():
            if verbose:
                print(f"\n✅ Advanced to Stage {curriculum.current_stage_idx}")

        episode_count += timesteps_this_stage // 2048  # Approximate episodes

    return model, curriculum
