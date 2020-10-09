data <- read.csv("./output_2.csv", sep = ";", dec=",")

#Histograms:
hist(data$X.Faces, xlim=c(0,max(data$X.Faces)), breaks = 1000, main="", xlab="Number of faces")  
hist(data$X.Vertices, xlim=c(0,max(data$X.Vertices)), breaks = 500, main="", xlab="Number of vertices")

#Histograms for the lower end of the spectrum:
hist(data$X.Faces, xlim=c(0,5000), breaks = 10000, main="", xlab="Number of faces")  
hist(data$X.Vertices, xlim=c(0,5000), breaks = 10000, main="", xlab="Number of vertices")

#Barplot for class value:
op <- par(mar = c(10,4,4,2) + 0.1)
barplot(height=table(data$Class), las=2, main="")
par(op)

#Averages:
avg_faces <- mean(data$X.Faces)
avg_verts <- mean(data$X.Vertices)
  
avg_AABBminX <- mean(data$AABB_min_X)
avg_AABBminY <- mean(data$AABB_min_Y)
avg_AABBminZ <- mean(data$AABB_min_Z)

avg_AABBmaxX <- mean(data$AABB_max_X)
avg_AABBmaxY <- mean(data$AABB_max_Y)
avg_AABBmaxZ <- mean(data$AABB_max_Z)

#Plot feature histogram:
range = 18 #Adjust range here #A3 = 18, D1 = 28, D2 = 38, D3 = 48, D4 = 58
min = 0 
max = 3.14 #Adjust the max of the histogram here.
bins = 10 #Adjust the bin size here.
names <- rep("", bins)
names[1] <- paste(min, (max - min)/10, sep=" - ")
names[bins] <- paste(9*(max - min)/10, max, sep=" - ")
rowid = 1337 #Adjust mesh-ID here
row <- data[data$ID == rowid, ] 
histo <- row[, range:(range+9)] 
barplot(as.numeric(histo), names.arg = names, cex.names=0.75)



