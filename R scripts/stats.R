data <- read.csv("./stats.csv", sep = ";", dec=",")

#Histograms:
hist(data$X.Faces, xlim=c(0,max(data$X.Faces)), breaks = 1000)  
hist(data$X.Vertices, xlim=c(0,max(data$X.Vertices)), breaks = 1000)

#Histograms for the lower end of the spectrum:
hist(data$X.Faces, xlim=c(0,5000), breaks = 10000)  
hist(data$X.Vertices, xlim=c(0,5000), breaks = 10000)

#Barplot for class value:
barplot(height=table(data$Class), las=2)

#Averages:
avg_faces <- mean(data$X.Faces)
avg_verts <- mean(data$X.Vertices)

avg_AABBminX <- mean(data$AABB.min.X)
avg_AABBminY <- mean(data$AABB.min.Y)
avg_AABBminZ <- mean(data$AABB.min.Z)

avg_AABBmaxX <- mean(data$AABB.max.X)
avg_AABBmaxY <- mean(data$AABB.max.Y)
avg_AABBmaxZ <- mean(data$AABB.max.Z)

