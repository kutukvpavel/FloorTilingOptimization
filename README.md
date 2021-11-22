# FloorTilingOptimization

This app tries to find an optimal arrangement of rectangular sheets (or tiles) of different sizes and thicknesses on a predefined support structure 
on the condition that the sheets can only be mounted on the beams. The sheets can be cut and/or flipped.

The support structure usually consists of several parallel beams (preferably they are oriented vertically w.r.t. screen coordinates convention, i.e. dy > dx).
The walls defined for the structure are not used during the optimization process, they are merely exported to the final DXF drawings and images for convinience.

**Optimization method**: genetic algorithm.

**Sheet placement (or tiling) algorithm** is fairly basic:

 - Read the stock from a CSV file, sheet index == row
 - Generate a random string of bits, each bit corresponds to the orientation of each sheet (1 = vertical, 0 = horizontal, in screen coords).
 If current orientation of the sheet (as read from file) doesn't match bit value, sides of the sheet's rectangle will be swapped.
 - Generate a random permutation of sheet indexes
 - Place the sheets onto the support beams according to the generated permutation
   - Placement starts at (0, 0)
   - Placement is performed column-wise, each column gets filled from top to bottom, columns are created from left to right (screen coordinates)
   - Left boundaries are formed by right sides of the sheets (they are not straight, apart from the first column)
   - Each sheet is intesected with all of the beams, the union rectangle of the resulting intersections defines how the sheet should be cut
   - Save 3 sheets: positioned version of the original sheet ("assessed"), cut and positioned version of it ("cut") and its largest child
 - Assess the placement (calculate fitness) using formula: 
 `F = [Covered Area] / [Total Support Area] - [Total Children Area] / [Total Sheet Area] + [Number of Joints of Sheets of Equal Thicknesses] / [Total Number of Joints]`
 - Combine the index string and the bit string into a single compound chromosome (with different operators, index string is permuted, whereas bit string is mainly mutated, and crossover between two strings is not allowed)
 - Perform genetic algorithm operations on this chromosome
 - Repeat untill satisfactory fitness is achieved or until step limit is reached
 - Cycle through N random seeds (sometimes a really bad seed leads to a non-converging genetic algorithm) if this option is specified
 
**Results** are saved as 3 images, DXF drawing, and 2 text files.
All the result correspond to the best chromosome (and not the last one).

The contents include:

 - Image of overlapped (not yet cut) sheets overlayed on the beams and walls
 - Image of cut sheets overlayed on the beams and walls
 - Image of children sheets overlayed on the beams and walls
 - DXF includes 4 layers (overlapped, cut, children, support), each sheet is tagged and grouped with its tag
 - Text file "order" includes chromosome contents, best fitness and covered area
 - Text file "evolution" contains fitness evolution in case you'd like to plot it
 
# Examples


