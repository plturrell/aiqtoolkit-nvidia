"""
Create visual diagram for NVIDIA ACE to Unity migration architecture
"""
import matplotlib.pyplot as plt
import matplotlib.patches as patches
from matplotlib.patches import FancyBboxPatch, ConnectionPatch
import numpy as np

# Create figure and axis
fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(16, 10))

# Current NVIDIA ACE Architecture (Left)
ax1.set_xlim(0, 10)
ax1.set_ylim(0, 10)
ax1.set_title('Current: NVIDIA ACE Architecture', fontsize=16, fontweight='bold')
ax1.axis('off')

# Add components
nvidia_components = [
    {'name': 'Client Application', 'pos': (5, 8.5), 'color': '#1E90FF'},
    {'name': 'NVIDIA ACE Server', 'pos': (5, 7), 'color': '#76B900'},
    {'name': 'NVIDIA Riva\n(ASR/TTS)', 'pos': (2, 5), 'color': '#76B900'},
    {'name': 'Audio2Face\n(Lip-sync)', 'pos': (5, 5), 'color': '#76B900'},
    {'name': 'NVIDIA NIM\n(LLM)', 'pos': (8, 5), 'color': '#76B900'},
    {'name': 'GPU Cluster\n(CUDA/TensorRT)', 'pos': (5, 3), 'color': '#FF6B35'},
    {'name': 'Hardware Infrastructure', 'pos': (5, 1), 'color': '#FF1744'}
]

for component in nvidia_components:
    box = FancyBboxPatch(
        (component['pos'][0] - 1.2, component['pos'][1] - 0.4),
        2.4, 0.8,
        boxstyle="round,pad=0.1",
        facecolor=component['color'],
        edgecolor='black',
        alpha=0.8
    )
    ax1.add_patch(box)
    ax1.text(component['pos'][0], component['pos'][1], component['name'],
             ha='center', va='center', fontsize=10, color='white', fontweight='bold')

# Add connections
connections = [
    ((5, 8.1), (5, 7.4)),
    ((5, 6.6), (2, 5.4)),
    ((5, 6.6), (5, 5.4)),
    ((5, 6.6), (8, 5.4)),
    ((2, 4.6), (5, 3.4)),
    ((5, 4.6), (5, 3.4)),
    ((8, 4.6), (5, 3.4)),
    ((5, 2.6), (5, 1.4))
]

for start, end in connections:
    arrow = ConnectionPatch(start, end, "data", "data",
                          arrowstyle="->", shrinkA=0, shrinkB=0,
                          mutation_scale=20, fc="black", lw=2)
    ax1.add_artist(arrow)

# Target Unity Architecture (Right)
ax2.set_xlim(0, 10)
ax2.set_ylim(0, 10)
ax2.set_title('Target: Unity + Cloud Architecture', fontsize=16, fontweight='bold')
ax2.axis('off')

# Add components
unity_components = [
    {'name': 'Unity Client', 'pos': (5, 8.5), 'color': '#1E90FF'},
    {'name': 'API Gateway', 'pos': (5, 7), 'color': '#9C27B0'},
    {'name': 'Cloud ASR\n(Google/Azure)', 'pos': (2, 5), 'color': '#4285F4'},
    {'name': 'Cloud TTS\n(AWS Polly)', 'pos': (5, 5), 'color': '#FF9900'},
    {'name': 'LLM API\n(OpenAI/Vertex)', 'pos': (8, 5), 'color': '#00BCD4'},
    {'name': 'Unity Rendering\n(Lipsync/Avatar)', 'pos': (5, 3), 'color': '#000000'},
    {'name': 'Cloud Infrastructure', 'pos': (5, 1), 'color': '#4CAF50'}
]

for component in unity_components:
    box = FancyBboxPatch(
        (component['pos'][0] - 1.2, component['pos'][1] - 0.4),
        2.4, 0.8,
        boxstyle="round,pad=0.1",
        facecolor=component['color'],
        edgecolor='black',
        alpha=0.8
    )
    ax2.add_patch(box)
    
    text_color = 'white' if component['color'] != '#000000' else 'white'
    ax2.text(component['pos'][0], component['pos'][1], component['name'],
             ha='center', va='center', fontsize=10, color=text_color, fontweight='bold')

# Add connections
unity_connections = [
    ((5, 8.1), (5, 7.4)),
    ((5, 6.6), (2, 5.4)),
    ((5, 6.6), (5, 5.4)),
    ((5, 6.6), (8, 5.4)),
    ((2, 4.6), (5, 3.4)),
    ((5, 4.6), (5, 3.4)),
    ((8, 4.6), (5, 6.6)),  # LLM back to gateway
    ((5, 2.6), (5, 1.4))
]

for start, end in unity_connections:
    arrow = ConnectionPatch(start, end, "data", "data",
                          arrowstyle="->", shrinkA=0, shrinkB=0,
                          mutation_scale=20, fc="black", lw=2)
    ax2.add_artist(arrow)

# Add migration arrow
migration_arrow = patches.FancyArrowPatch(
    (10.5, 5), (12.5, 5),
    connectionstyle="arc3", 
    arrowstyle='->',
    mutation_scale=50,
    color='red',
    linewidth=3
)

# Add title
plt.suptitle('NVIDIA ACE to Unity Migration Architecture', fontsize=20, fontweight='bold', y=0.98)

# Add phase labels
phases = [
    {'text': 'Phase 1: Backend Retention', 'y': 0.15, 'color': '#FF6B35'},
    {'text': 'Phase 2: Frontend Migration', 'y': 0.10, 'color': '#9C27B0'},
    {'text': 'Phase 3: Full Unity Deployment', 'y': 0.05, 'color': '#4CAF50'}
]

for phase in phases:
    plt.figtext(0.5, phase['y'], phase['text'], 
                ha='center', fontsize=14, fontweight='bold', color=phase['color'])

# Add cost comparison
cost_text = 'Cost Reduction: 60-70%\nHardware Requirements: Minimal'
plt.figtext(0.95, 0.85, cost_text, 
            ha='right', fontsize=12, bbox=dict(boxstyle="round", facecolor='lightgreen', alpha=0.7))

plt.tight_layout()
plt.savefig('/Users/apple/projects/AIQToolkit/src/aiq/digital_human/migration/migration_architecture.png', dpi=300, bbox_inches='tight')
plt.close()

# Create timeline diagram
fig, ax = plt.subplots(figsize=(14, 8))

# Timeline data
phases = [
    {'name': 'Phase 1: Backend Retention', 'start': 0, 'duration': 4, 'color': '#FF6B35'},
    {'name': 'Phase 2: Frontend Migration', 'start': 4, 'duration': 8, 'color': '#9C27B0'},
    {'name': 'Phase 3: Full Unity Deploy', 'start': 12, 'duration': 8, 'color': '#4CAF50'}
]

tasks = [
    # Phase 1
    {'name': 'Project Setup', 'phase': 0, 'start': 0, 'duration': 1},
    {'name': 'API Abstraction Layer', 'phase': 0, 'start': 1, 'duration': 1},
    {'name': 'Unity Bridge', 'phase': 0, 'start': 2, 'duration': 1},
    {'name': 'Initial Testing', 'phase': 0, 'start': 3, 'duration': 1},
    
    # Phase 2
    {'name': 'Avatar Integration', 'phase': 1, 'start': 4, 'duration': 2},
    {'name': 'Lipsync Implementation', 'phase': 1, 'start': 6, 'duration': 2},
    {'name': 'Animation System', 'phase': 1, 'start': 8, 'duration': 2},
    {'name': 'Performance Testing', 'phase': 1, 'start': 10, 'duration': 2},
    
    # Phase 3
    {'name': 'Backend Migration', 'phase': 2, 'start': 12, 'duration': 3},
    {'name': 'Cloud Deployment', 'phase': 2, 'start': 15, 'duration': 2},
    {'name': 'Final Testing', 'phase': 2, 'start': 17, 'duration': 2},
    {'name': 'Go-Live', 'phase': 2, 'start': 19, 'duration': 1},
]

# Plot phases
for i, phase in enumerate(phases):
    rect = patches.Rectangle((phase['start'], i*3), phase['duration'], 0.8,
                           facecolor=phase['color'], alpha=0.3, edgecolor='black')
    ax.add_patch(rect)
    ax.text(phase['start'] + phase['duration']/2, i*3 + 0.4, phase['name'],
            ha='center', va='center', fontweight='bold', fontsize=12)

# Plot tasks
for task in tasks:
    y_pos = task['phase'] * 3 + 1.2 + (task['start'] % 2) * 0.4
    rect = patches.Rectangle((task['start'], y_pos), task['duration'], 0.3,
                           facecolor=phases[task['phase']]['color'], 
                           alpha=0.8, edgecolor='black')
    ax.add_patch(rect)
    ax.text(task['start'] + task['duration']/2, y_pos + 0.15, task['name'],
            ha='center', va='center', fontsize=9, rotation=0)

# Customize plot
ax.set_xlim(0, 20)
ax.set_ylim(-0.5, 9)
ax.set_xlabel('Weeks', fontsize=14, fontweight='bold')
ax.set_title('NVIDIA ACE to Unity Migration Timeline', fontsize=18, fontweight='bold')
ax.grid(True, axis='x', alpha=0.3)
ax.set_xticks(range(0, 21, 2))
ax.set_yticks([])

# Add milestones
milestones = [
    {'week': 4, 'text': 'Unity Frontend Ready'},
    {'week': 12, 'text': 'Lipsync Complete'},
    {'week': 20, 'text': 'Full Migration'}
]

for milestone in milestones:
    ax.axvline(x=milestone['week'], color='red', linestyle='--', alpha=0.7)
    ax.text(milestone['week'], 8.5, milestone['text'], rotation=90,
            va='bottom', ha='center', fontweight='bold', color='red')

plt.tight_layout()
plt.savefig('/Users/apple/projects/AIQToolkit/src/aiq/digital_human/migration/migration_timeline.png', dpi=300, bbox_inches='tight')
plt.close()

print("Migration diagrams created successfully!")
print("- Architecture comparison: migration_architecture.png")
print("- Timeline diagram: migration_timeline.png")