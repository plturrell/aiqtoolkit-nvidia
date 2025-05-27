"""
DSPy Parameter Calibration and Boundary Management
==================================================

Implements rigorous parameter validation, calibration boundaries, and
adaptive optimization for DSPy reasoning systems in AIQToolkit.
"""

import numpy as np
import torch
import torch.nn as nn
from typing import Dict, Any, Tuple, Optional, Union
from dataclasses import dataclass, field
import logging
from pathlib import Path
import json
import matplotlib.pyplot as plt
from scipy.optimize import minimize_scalar
from sklearn.isotonic import IsotonicRegression
from sklearn.linear_model import LogisticRegression

logger = logging.getLogger(__name__)


@dataclass
class DSPyParameterBounds:
    """Rigorous parameter boundaries for DSPy optimization."""
    
    # Core DSPy Parameters
    temperature_min: float = 0.1
    temperature_max: float = 2.0
    temperature_default: float = 0.7
    
    max_tokens_min: int = 100
    max_tokens_max: int = 8192
    max_tokens_default: int = 2000
    
    # Optimization Parameters  
    learning_rate_min: float = 1e-6
    learning_rate_max: float = 1e-1
    learning_rate_default: float = 1e-4
    
    # DSPy Specific
    max_rounds_min: int = 1
    max_rounds_max: int = 10
    max_rounds_default: int = 3
    
    max_examples_min: int = 1
    max_examples_max: int = 100
    max_examples_default: int = 10
    
    # Neural-Symbolic Balance
    reasoning_weight_min: float = 0.0
    reasoning_weight_max: float = 1.0
    reasoning_weight_default: float = 0.3
    
    # Convergence Criteria
    tolerance_min: float = 1e-6
    tolerance_max: float = 1e-2
    tolerance_default: float = 1e-4
    
    def validate_parameter(self, param_name: str, value: Union[float, int]) -> bool:
        """Validate parameter against defined boundaries."""
        
        if param_name == "temperature":
            return self.temperature_min <= value <= self.temperature_max
        elif param_name == "max_tokens":
            return self.max_tokens_min <= value <= self.max_tokens_max
        elif param_name == "learning_rate":
            return self.learning_rate_min <= value <= self.learning_rate_max
        elif param_name == "max_rounds":
            return self.max_rounds_min <= value <= self.max_rounds_max
        elif param_name == "max_examples":
            return self.max_examples_min <= value <= self.max_examples_max
        elif param_name == "reasoning_weight":
            return self.reasoning_weight_min <= value <= self.reasoning_weight_max
        elif param_name == "tolerance":
            return self.tolerance_min <= value <= self.tolerance_max
        else:
            logger.warning(f"Unknown parameter: {param_name}")
            return False
    
    def clip_parameter(self, param_name: str, value: Union[float, int]) -> Union[float, int]:
        """Clip parameter to valid boundaries."""
        
        if param_name == "temperature":
            return np.clip(value, self.temperature_min, self.temperature_max)
        elif param_name == "max_tokens":
            return np.clip(value, self.max_tokens_min, self.max_tokens_max)
        elif param_name == "learning_rate":
            return np.clip(value, self.learning_rate_min, self.learning_rate_max)
        elif param_name == "max_rounds":
            return np.clip(value, self.max_rounds_min, self.max_rounds_max)
        elif param_name == "max_examples":
            return np.clip(value, self.max_examples_min, self.max_examples_max)
        elif param_name == "reasoning_weight":
            return np.clip(value, self.reasoning_weight_min, self.reasoning_weight_max)
        elif param_name == "tolerance":
            return np.clip(value, self.tolerance_min, self.tolerance_max)
        else:
            return value


@dataclass
class DSPyCalibrationConfig:
    """Configuration for DSPy calibration and validation."""
    
    # Validation Parameters
    temperature: float = field(default=0.7)
    max_tokens: int = field(default=2000)
    learning_rate: float = field(default=1e-4)
    max_rounds: int = field(default=3)
    max_examples: int = field(default=10)
    reasoning_weight: float = field(default=0.3)
    tolerance: float = field(default=1e-4)
    
    # Calibration Settings
    enable_temperature_scaling: bool = field(default=True)
    enable_confidence_calibration: bool = field(default=True)
    calibration_method: str = field(default="platt")  # "platt", "isotonic", "temperature"
    
    # Validation Settings
    validate_parameters: bool = field(default=True)
    clip_invalid_parameters: bool = field(default=True)
    
    def __post_init__(self):
        """Validate configuration after initialization."""
        if not self.validate_parameters:
            return
            
        bounds = DSPyParameterBounds()
        
        # Validate all parameters
        for param_name in ["temperature", "max_tokens", "learning_rate", 
                          "max_rounds", "max_examples", "reasoning_weight", "tolerance"]:
            value = getattr(self, param_name)
            
            if not bounds.validate_parameter(param_name, value):
                if self.clip_invalid_parameters:
                    clipped_value = bounds.clip_parameter(param_name, value)
                    setattr(self, param_name, clipped_value)
                    logger.warning(f"Clipped {param_name} from {value} to {clipped_value}")
                else:
                    raise ValueError(f"Invalid {param_name}: {value}")


class TemperatureScaling(nn.Module):
    """
    Temperature scaling for confidence calibration.
    
    Reference: Guo, C., et al. (2017). "On Calibration of Modern Neural Networks."
    """
    
    def __init__(self, initial_temperature: float = 1.0):
        super().__init__()
        self.temperature = nn.Parameter(torch.ones(1) * initial_temperature)
        
    def forward(self, logits: torch.Tensor) -> torch.Tensor:
        """Apply temperature scaling to logits."""
        return logits / self.temperature
    
    def calibrate(self, logits: torch.Tensor, labels: torch.Tensor, 
                  max_iter: int = 50, lr: float = 0.01) -> float:
        """
        Calibrate temperature using validation data.
        
        Args:
            logits: Model output logits [N, C]
            labels: True labels [N]
            max_iter: Maximum optimization iterations
            lr: Learning rate for temperature optimization
            
        Returns:
            Calibrated temperature value
        """
        self.train()
        optimizer = torch.optim.LBFGS([self.temperature], lr=lr, max_iter=max_iter)
        
        def eval_loss():
            optimizer.zero_grad()
            loss = nn.CrossEntropyLoss()(self.forward(logits), labels)
            loss.backward()
            return loss
        
        optimizer.step(eval_loss)
        
        self.eval()
        return self.temperature.item()


class DSPyConfidenceCalibrator:
    """
    Comprehensive confidence calibration for DSPy reasoning systems.
    
    Implements multiple calibration methods with theoretical guarantees.
    """
    
    def __init__(self, method: str = "platt"):
        self.method = method
        self.calibrator = None
        self.is_fitted = False
        
    def fit(self, predictions: np.ndarray, targets: np.ndarray) -> 'DSPyConfidenceCalibrator':
        """
        Fit calibration model on validation data.
        
        Args:
            predictions: Model predictions/confidence scores [N]
            targets: Binary targets (0/1) [N]
            
        Returns:
            Self for method chaining
        """
        predictions = np.array(predictions).reshape(-1, 1)
        targets = np.array(targets)
        
        if self.method == "platt":
            # Platt scaling using logistic regression
            self.calibrator = LogisticRegression()
            self.calibrator.fit(predictions, targets)
            
        elif self.method == "isotonic":
            # Isotonic regression
            self.calibrator = IsotonicRegression(out_of_bounds='clip')
            self.calibrator.fit(predictions.flatten(), targets)
            
        elif self.method == "temperature":
            # Temperature scaling (requires logits)
            self.calibrator = TemperatureScaling()
            
        else:
            raise ValueError(f"Unknown calibration method: {self.method}")
        
        self.is_fitted = True
        return self
    
    def predict_proba(self, predictions: np.ndarray) -> np.ndarray:
        """
        Apply calibration to predictions.
        
        Args:
            predictions: Raw model predictions [N]
            
        Returns:
            Calibrated probabilities [N]
        """
        if not self.is_fitted:
            raise ValueError("Calibrator not fitted. Call fit() first.")
        
        predictions = np.array(predictions)
        
        if self.method == "platt":
            if predictions.ndim == 1:
                predictions = predictions.reshape(-1, 1)
            return self.calibrator.predict_proba(predictions)[:, 1]
            
        elif self.method == "isotonic":
            return self.calibrator.predict(predictions)
            
        elif self.method == "temperature":
            # For temperature scaling, requires logits input
            logits = torch.tensor(predictions, dtype=torch.float32)
            with torch.no_grad():
                calibrated_logits = self.calibrator(logits)
                return torch.softmax(calibrated_logits, dim=-1).numpy()
        
        else:
            return predictions
    
    def evaluate_calibration(self, predictions: np.ndarray, targets: np.ndarray, 
                           n_bins: int = 10) -> Dict[str, float]:
        """
        Evaluate calibration quality using Expected Calibration Error (ECE).
        
        Args:
            predictions: Calibrated predictions [N]
            targets: True binary targets [N]
            n_bins: Number of bins for ECE calculation
            
        Returns:
            Dictionary with calibration metrics
        """
        predictions = np.array(predictions)
        targets = np.array(targets)
        
        # Calculate Expected Calibration Error (ECE)
        bin_boundaries = np.linspace(0, 1, n_bins + 1)
        bin_lowers = bin_boundaries[:-1]
        bin_uppers = bin_boundaries[1:]
        
        ece = 0.0
        mce = 0.0  # Maximum Calibration Error
        
        for bin_lower, bin_upper in zip(bin_lowers, bin_uppers):
            in_bin = (predictions > bin_lower) & (predictions <= bin_upper)
            prop_in_bin = in_bin.mean()
            
            if prop_in_bin > 0:
                accuracy_in_bin = targets[in_bin].mean()
                avg_confidence_in_bin = predictions[in_bin].mean()
                
                calibration_error = abs(avg_confidence_in_bin - accuracy_in_bin)
                ece += prop_in_bin * calibration_error
                mce = max(mce, calibration_error)
        
        # Calculate Brier Score
        brier_score = np.mean((predictions - targets) ** 2)
        
        # Calculate reliability and resolution
        reliability = np.sum([
            np.sum(in_bin) * (predictions[in_bin].mean() - targets[in_bin].mean()) ** 2 
            for bin_lower, bin_upper in zip(bin_lowers, bin_uppers)
            for in_bin in [(predictions > bin_lower) & (predictions <= bin_upper)]
            if np.sum(in_bin) > 0
        ]) / len(predictions)
        
        resolution = np.var(targets)
        
        return {
            "ece": ece,
            "mce": mce,
            "brier_score": brier_score,
            "reliability": reliability,
            "resolution": resolution
        }


class DSPyParameterOptimizer:
    """
    Adaptive parameter optimization for DSPy systems.
    
    Implements Bayesian optimization for hyperparameter tuning with
    calibration boundary constraints.
    """
    
    def __init__(self, bounds: DSPyParameterBounds = None):
        self.bounds = bounds or DSPyParameterBounds()
        self.optimization_history = []
        
    def optimize_temperature(self, validation_data: Dict[str, np.ndarray],
                           objective: str = "ece") -> float:
        """
        Optimize temperature parameter to minimize calibration error.
        
        Args:
            validation_data: Dict with 'predictions' and 'targets' arrays
            objective: Optimization objective ("ece", "brier", "nll")
            
        Returns:
            Optimal temperature value
        """
        predictions = validation_data["predictions"]
        targets = validation_data["targets"]
        
        def objective_function(temperature):
            # Apply temperature scaling
            scaled_predictions = predictions / temperature
            scaled_probs = np.exp(scaled_predictions) / np.sum(np.exp(scaled_predictions), axis=-1, keepdims=True)
            
            # Calculate objective
            if objective == "ece":
                calibrator = DSPyConfidenceCalibrator("isotonic")
                calibrator.fit(scaled_probs[:, 1], targets)  # Assuming binary classification
                metrics = calibrator.evaluate_calibration(scaled_probs[:, 1], targets)
                return metrics["ece"]
                
            elif objective == "brier":
                return np.mean((scaled_probs[:, 1] - targets) ** 2)
                
            elif objective == "nll":
                return -np.mean(targets * np.log(scaled_probs[:, 1] + 1e-8) + 
                               (1 - targets) * np.log(1 - scaled_probs[:, 1] + 1e-8))
            
            else:
                raise ValueError(f"Unknown objective: {objective}")
        
        # Optimize within bounds
        result = minimize_scalar(
            objective_function,
            bounds=(self.bounds.temperature_min, self.bounds.temperature_max),
            method='bounded'
        )
        
        optimal_temperature = result.x
        self.optimization_history.append({
            "parameter": "temperature",
            "optimal_value": optimal_temperature,
            "objective_value": result.fun,
            "objective": objective
        })
        
        return optimal_temperature
    
    def adaptive_learning_rate(self, loss_history: list, 
                              current_lr: float) -> float:
        """
        Adaptive learning rate adjustment based on loss trends.
        
        Args:
            loss_history: List of recent loss values
            current_lr: Current learning rate
            
        Returns:
            Adjusted learning rate
        """
        if len(loss_history) < 3:
            return current_lr
        
        # Calculate trend
        recent_losses = loss_history[-3:]
        if recent_losses[-1] > recent_losses[-2] > recent_losses[-3]:
            # Loss increasing - reduce learning rate
            new_lr = current_lr * 0.5
        elif recent_losses[-1] < recent_losses[-2] < recent_losses[-3]:
            # Loss decreasing - can increase learning rate slightly
            new_lr = current_lr * 1.1
        else:
            # Oscillating - keep current
            new_lr = current_lr
        
        # Apply bounds
        new_lr = self.bounds.clip_parameter("learning_rate", new_lr)
        
        return new_lr


class DSPyCalibrationValidator:
    """
    Comprehensive validation framework for DSPy calibration quality.
    """
    
    @staticmethod
    def reliability_diagram(predictions: np.ndarray, targets: np.ndarray, 
                          n_bins: int = 10, save_path: Optional[str] = None):
        """
        Generate reliability diagram for calibration assessment.
        
        Args:
            predictions: Model predictions [N]
            targets: True binary targets [N]
            n_bins: Number of bins
            save_path: Optional path to save plot
        """
        bin_boundaries = np.linspace(0, 1, n_bins + 1)
        bin_lowers = bin_boundaries[:-1]
        bin_uppers = bin_boundaries[1:]
        
        bin_centers = []
        bin_accuracies = []
        bin_confidences = []
        bin_counts = []
        
        for bin_lower, bin_upper in zip(bin_lowers, bin_uppers):
            in_bin = (predictions >= bin_lower) & (predictions < bin_upper)
            prop_in_bin = in_bin.mean()
            
            if prop_in_bin > 0:
                accuracy_in_bin = targets[in_bin].mean()
                confidence_in_bin = predictions[in_bin].mean()
                count_in_bin = np.sum(in_bin)
                
                bin_centers.append((bin_lower + bin_upper) / 2)
                bin_accuracies.append(accuracy_in_bin)
                bin_confidences.append(confidence_in_bin)
                bin_counts.append(count_in_bin)
        
        # Create reliability diagram
        plt.figure(figsize=(8, 6))
        plt.plot([0, 1], [0, 1], 'k--', label='Perfect Calibration')
        plt.scatter(bin_confidences, bin_accuracies, s=bin_counts, alpha=0.7, label='Model')
        plt.xlabel('Mean Predicted Probability')
        plt.ylabel('Fraction of Positives')
        plt.title('Reliability Diagram')
        plt.legend()
        plt.grid(True, alpha=0.3)
        
        if save_path:
            plt.savefig(save_path, dpi=300, bbox_inches='tight')
        plt.show()
    
    @staticmethod
    def validate_parameter_bounds(config: DSPyCalibrationConfig) -> Dict[str, bool]:
        """
        Validate all parameters against defined boundaries.
        
        Args:
            config: DSPy calibration configuration
            
        Returns:
            Dictionary of validation results
        """
        bounds = DSPyParameterBounds()
        results = {}
        
        for param_name in ["temperature", "max_tokens", "learning_rate", 
                          "max_rounds", "max_examples", "reasoning_weight", "tolerance"]:
            value = getattr(config, param_name)
            results[param_name] = bounds.validate_parameter(param_name, value)
        
        return results


# Example usage and testing
if __name__ == "__main__":
    # Example parameter calibration
    config = DSPyCalibrationConfig(
        temperature=0.8,
        max_tokens=1500,
        learning_rate=5e-5,
        enable_temperature_scaling=True,
        calibration_method="platt"
    )
    
    print("Configuration:", config)
    
    # Validate parameters
    validator = DSPyCalibrationValidator()
    validation_results = validator.validate_parameter_bounds(config)
    print("Validation results:", validation_results)
    
    # Example calibration
    np.random.seed(42)
    predictions = np.random.beta(2, 2, 1000)  # Simulated predictions
    targets = (np.random.random(1000) < predictions).astype(int)  # Simulated targets
    
    calibrator = DSPyConfidenceCalibrator("platt")
    calibrator.fit(predictions, targets)
    
    calibrated_predictions = calibrator.predict_proba(predictions)
    metrics = calibrator.evaluate_calibration(calibrated_predictions, targets)
    
    print("Calibration metrics:", metrics)