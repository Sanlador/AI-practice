#This program implements the RDFS and IDA* to solve a 15 puzzle
import random
import numpy as np

class game:
	board = np.zeros((4,4))
	direction = {
		"up": (-1, 0),
		"down": (1, 0),
		"right": (0, 1),
		"left": (0, -1)
	}

	#create a 4x4 grid with values 0 to 15 randomly assigned (0 is considered the empty space)
	def __init__(self):
		count = 0
		for i in range(4):
			for j in range(4):
				self.board[i][j] = count
				count += 1
		
	#locates the index for zero for the purposes of playing the game
	def findZero(self):
		loc = np.where(self.board == 0)
		return (loc[0],loc[1])
		
	#perform a move on the board
	def move(self, direction):
		position  = self.findZero()
		move = (position[0] + self.direction[direction][0], position[1] + self.direction[direction][1])
		#check for illegal moves
		if position[0] == 0 and direction == "up":
			return
		if position[0] == 3 and direction == "down":
			return
		if position[1] == 0 and direction == "left":
			return
		if position[1] == 3 and direction == "right":
			return
		
		self.board[position] = self.board[move]
		self.board[move] = 0
		print(self.board)
		return True
		
	def goalTest(self):
		count = 1
		for i in range(4):
			for j in range(4):
				if i == 3 and j == 3:
					return True
				elif self.board[i][j] != count:
					return False
				count += 1
				
	def scramble(self, m):
		compass = ["up", "down", "left", "right"]
		for i in range(m):
			control = False
			while control == False:
				if self.move(compass[random.randint(0,3)]):
					control = True
		

x = game()
x.scramble(10)