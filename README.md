# ctb_calc
ctb_calc is a tool that calculates the difficulty of a beatmap when played in Catch the Beat. Simply drag your .osu file onto the executable, and as long as the file is in either osu-standard mode or CTB mode, then ctb_calc will spit out the relevant information

Currently the .osu file parser is being worked on, and no actual difficulty calculation has been programmed yet. The calculator can currently parse circles and linear sliders (as spinners will be ignored in the actual difficulty calculation). Bezier, Catmull and Passthrough sliders are expected to be implemented next.

The first goal of the project is to have the program be able to calculate the average of the top 10% largest "speeds" (change in position over change in time between two notes) in a given beatmap.
