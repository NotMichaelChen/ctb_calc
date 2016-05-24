# ctb_calc
ctb_calc is a tool that calculates the difficulty of a beatmap when played in Catch the Beat. Simply drag your .osu file onto the executable, and as long as the file is in either osu-standard mode or CTB mode, then ctb_calc will spit out the relevant information

The calculator can now parse all hit objects, including hitcircles, sliders (every type), and spinners (which are ignored). Current work now involves optimizing slider calculations, and implementing actual difficulty calculation. For now, the calculator simply prints out the average of the top 10% largest "speeds" (change in position over change in time between two notes) in a given beatmap.
