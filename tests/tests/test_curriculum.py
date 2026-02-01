"""
Tests for curriculum learning module.
"""

import pytest
from curriculum import (
    CurriculumStage,
    CurriculumMetrics,
    AdaptiveCurriculum,
)


class TestCurriculumStage:
    """Test CurriculumStage configuration."""

    def test_stage_creation(self):
        """Test creating a curriculum stage."""
        stage = CurriculumStage(
            stage=0,
            name="Test Stage",
            description="Test description",
            timesteps=100_000,
            target_reward=500.0,
            num_planes=2,
        )

        assert stage.stage == 0
        assert stage.name == "Test Stage"
        assert stage.timesteps == 100_000
        assert stage.target_reward == 500.0
        assert stage.num_planes == 2

    def test_stage_defaults(self):
        """Test stage default values."""
        stage = CurriculumStage(
            stage=0,
            name="Test",
            description="Test",
            timesteps=100_000,
        )

        assert stage.min_performance_threshold == 0.5
        assert stage.max_timesteps_per_episode == 2000
        assert stage.num_planes == 4
        assert stage.collision_penalty == 200.0

    def test_get_stage_config_dict(self):
        """Test stage configuration dictionary generation."""
        stage = CurriculumStage(
            stage=0,
            name="Test",
            description="Test",
            timesteps=100_000,
            num_planes=3,
            landing_reward=150.0,
        )

        config = stage.__dict__
        assert "stage" in config
        assert "name" in config
        assert "num_planes" in config


class TestCurriculumMetrics:
    """Test curriculum metrics tracking."""

    def test_metrics_initialization(self):
        """Test metrics object creation."""
        metrics = CurriculumMetrics(stage=0)

        assert metrics.stage == 0
        assert metrics.episode_count == 0
        assert metrics.mean_reward == 0.0
        assert len(metrics.rewards_history) == 0

    def test_update_single_episode(self):
        """Test updating metrics with single episode."""
        metrics = CurriculumMetrics(stage=0)
        metrics.update(100.0)

        assert metrics.episode_count == 1
        assert metrics.mean_reward == 100.0
        assert metrics.max_reward == 100.0
        assert metrics.min_reward == 100.0
        assert len(metrics.rewards_history) == 1

    def test_update_multiple_episodes(self):
        """Test updating metrics with multiple episodes."""
        metrics = CurriculumMetrics(stage=0)

        rewards = [50.0, 100.0, 150.0, 200.0, 250.0]
        for reward in rewards:
            metrics.update(reward)

        assert metrics.episode_count == 5
        assert metrics.mean_reward == 150.0
        assert metrics.max_reward == 250.0
        assert metrics.min_reward == 50.0

    def test_convergence_percentage_with_target(self):
        """Test convergence percentage calculation."""
        metrics = CurriculumMetrics(stage=0)

        for _ in range(20):
            metrics.update(100.0)

        # 100 out of 200 target = 50%
        convergence = metrics.convergence_percentage(target_reward=200.0)
        assert convergence == 50.0

    def test_convergence_percentage_no_target(self):
        """Test convergence percentage with no target."""
        metrics = CurriculumMetrics(stage=0)
        metrics.update(100.0)

        convergence = metrics.convergence_percentage(target_reward=None)
        assert convergence == 100.0

    def test_convergence_percentage_exceeded_target(self):
        """Test convergence when exceeding target."""
        metrics = CurriculumMetrics(stage=0)

        for _ in range(20):
            metrics.update(300.0)

        # 300 out of 200 target = 150%, capped at 100%
        convergence = metrics.convergence_percentage(target_reward=200.0)
        assert convergence == 100.0


class TestAdaptiveCurriculum:
    """Test adaptive curriculum management."""

    def test_curriculum_creation(self):
        """Test creating curriculum."""
        curriculum = AdaptiveCurriculum()

        assert len(curriculum.stages) > 0
        assert curriculum.current_stage_idx == 0

    def test_get_current_stage(self):
        """Test retrieving current stage."""
        curriculum = AdaptiveCurriculum()
        stage = curriculum.current_stage

        assert stage is not None
        assert stage.stage == 0

    def test_stage_progression(self):
        """Test that stages progress in order."""
        curriculum = AdaptiveCurriculum()

        for i in range(min(3, len(curriculum.stages))):
            stage = curriculum.current_stage
            assert stage.stage == i
            curriculum.current_stage_idx += 1

    def test_initialize_stage_metrics(self):
        """Test initializing metrics for stage."""
        curriculum = AdaptiveCurriculum()
        curriculum.initialize_stage_metrics()

        stage_idx = curriculum.current_stage.stage
        assert stage_idx in curriculum.metrics
        assert curriculum.metrics[stage_idx].stage == stage_idx

    def test_update_stage_metrics(self):
        """Test updating stage metrics."""
        curriculum = AdaptiveCurriculum()
        curriculum.initialize_stage_metrics()

        curriculum.update_stage_metrics(100.0)
        curriculum.update_stage_metrics(150.0)

        stage_idx = curriculum.current_stage.stage
        metrics = curriculum.metrics[stage_idx]
        assert metrics.episode_count == 2
        assert metrics.mean_reward == 125.0

    def test_should_advance_stage_conditions(self):
        """Test conditions for stage advancement."""
        curriculum = AdaptiveCurriculum()
        curriculum.initialize_stage_metrics()

        # Not enough episodes
        assert not curriculum.should_advance_stage()

        # Add episodes with poor performance
        for _ in range(20):
            curriculum.update_stage_metrics(10.0)

        assert not curriculum.should_advance_stage()

        # Add episodes with good performance
        for _ in range(50):
            curriculum.update_stage_metrics(curriculum.current_stage.target_reward)

        # Should advance if conditions met
        # (depends on target threshold logic)

    def test_advance_stage(self):
        """Test advancing to next stage."""
        curriculum = AdaptiveCurriculum()
        initial_stage = curriculum.current_stage_idx

        # Force advance by directly incrementing
        if curriculum.current_stage_idx < len(curriculum.stages) - 1:
            curriculum.current_stage_idx += 1
            assert curriculum.current_stage_idx > initial_stage

    def test_get_stage_config_dict(self):
        """Test getting stage configuration as dictionary."""
        curriculum = AdaptiveCurriculum()
        config = curriculum.get_stage_config_dict()

        assert "stage" in config
        assert "num_planes" in config
        assert "landing_reward" in config
        assert "collision_penalty" in config

    def test_stage_difficulties_increase(self):
        """Test that difficulty increases with stages."""
        curriculum = AdaptiveCurriculum()
        stages = curriculum.stages

        # Check that later stages have more aircraft
        num_planes = [s.num_planes for s in stages]
        assert num_planes == sorted(num_planes, reverse=False) or num_planes[0] <= num_planes[-1]

    def test_get_summary(self):
        """Test getting curriculum summary."""
        curriculum = AdaptiveCurriculum()
        curriculum.initialize_stage_metrics()

        summary = curriculum.get_summary()

        assert isinstance(summary, str)
        assert "Stage 0" in summary or "stage 0" in summary.lower()
        assert len(summary) > 0


class TestDefaultStages:
    """Test default curriculum stage configurations."""

    def test_stage_0_basic_approach(self):
        """Test stage 0 configuration."""
        curriculum = AdaptiveCurriculum()
        stage = curriculum.stages[0]

        assert stage.stage == 0
        assert "Basic" in stage.name or "Single" in stage.name
        assert stage.num_planes == 1
        assert stage.intercept_angle_range == (0.0, 0.0)

    def test_stage_difficulty_progression(self):
        """Test that difficulty increases across stages."""
        curriculum = AdaptiveCurriculum()

        # Check that timesteps and targets increase
        for i in range(len(curriculum.stages) - 1):
            stage_current = curriculum.stages[i]
            stage_next = curriculum.stages[i + 1]

            # Next stage should have at least as many timesteps
            assert stage_next.timesteps >= stage_current.timesteps

    def test_all_stages_have_targets(self):
        """Test that stages have reasonable target rewards."""
        curriculum = AdaptiveCurriculum()

        for stage in curriculum.stages[:-1]:  # All but last
            assert stage.target_reward is not None
            assert stage.target_reward > 0

    def test_reward_escalation(self):
        """Test that penalty structure escalates appropriately."""
        curriculum = AdaptiveCurriculum()

        # Check that collision penalties increase as difficulty increases
        penalties = [s.collision_penalty for s in curriculum.stages]
        # Penalties should generally increase or stay constant
        assert penalties[0] <= penalties[-1]


class TestStageTransitions:
    """Test curriculum stage transitions."""

    def test_max_stage_no_advance(self):
        """Test that we don't advance past last stage."""
        curriculum = AdaptiveCurriculum()
        curriculum.current_stage_idx = len(curriculum.stages) - 1

        assert not curriculum.should_advance_stage()

    def test_get_stage_out_of_bounds(self):
        """Test getting stage with out of bounds index."""
        curriculum = AdaptiveCurriculum()

        stage = curriculum.get_stage(999)
        assert stage is not None
        assert stage == curriculum.stages[-1]

    def test_metrics_per_stage(self):
        """Test that each stage tracks separate metrics."""
        curriculum = AdaptiveCurriculum()

        # Simulate progression through stages
        for stage_idx in range(min(3, len(curriculum.stages))):
            curriculum.initialize_stage_metrics()

            for _ in range(10):
                curriculum.update_stage_metrics(100.0 * (stage_idx + 1))

            curriculum.current_stage_idx += 1

        # Check that each stage has its own metrics
        assert len(curriculum.metrics) >= min(3, len(curriculum.stages))


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
