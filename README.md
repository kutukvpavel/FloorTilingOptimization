# FloorTilingOptimization

This app tries to find an optimal arrangement of rectangular sheets (or tiles) of different sizes and thicknesses on a predefined support structure 
on the condition that the sheets can only be mounted on the beams. The sheets can be cut and/or flipped. This is sort of a 2D *cutting stock* problem.

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
 `F = [Covered Area] / [Total Support Area] - [Total Children Area] / [Total Sheet Area] + [Joint Factor Weight = 0.1] [Number of Joints of Sheets of Equal Thicknesses] / [Total Number of Joints]`
 - Combine the index string and the bit string into a single compound chromosome (with different operators, index string is permuted, whereas bit string is mainly mutated, and crossover between two strings is not allowed)
 - Perform genetic algorithm operations on this chromosome
 - Repeat untill satisfactory fitness is achieved or until step limit is reached
 - Cycle through N random seeds (sometimes a really bad seed leads to a non-converging genetic algorithm) if this option is specified
 
**Results** are saved as 4 PNG images (scaled to 2K resolution), a DXF drawing, and 2 text files.
All the result correspond to the best chromosome (and not the last one).

The contents include:

 - An image of overlapped (not yet cut) sheets overlayed on the beams and walls
 - An image of cut sheets overlayed on the beams and walls
 - An image of children sheets overlayed on the beams and walls
 - An image of the support structure (as loaded from the CSV file)
 - DXF includes 4 layers (overlapped, cut, children, support), each sheet is tagged and grouped with its tag
 - Text file "order" includes chromosome contents, best fitness and covered area
 - Text file "evolution" contains fitness evolution in case you'd like to plot it

**Additional features:** RectpackSharp interface (legacy).
 
# Examples
Input CSV files: look inside the project folder.

CLI arguments: bare minimum is `-b "beams.csv" -s "sheets.csv"`, you can run it without any arguments to see help

Output: https://imgur.com/a/Nt7OMEQ

# Dependecies

This project wouldn't have been possible without the following awesome packages:

 - https://github.com/commandlineparser/commandline
 - https://joshclose.github.io/CsvHelper/
 - https://github.com/giacomelli/GeneticSharp
 - https://github.com/haplokuon/netDxf
 - https://github.com/ThomasMiz/RectpackSharp (though now it's a legacy inside this project)
 - https://github.com/SixLabors/ImageSharp (+ currently beta package ImageSharp.Drawing)
