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
		count = 1
		for i in range(4):
			for j in range(4):
				self.board[i][j] = count
				count += 1
		self.board[3][3] = 0
		
	#locates the index for zero for the purposes of playing the game
	def findZero(self):
		loc = np.where(self.board == 0)
		return (loc[0],loc[1])
		
	#returns a board position for a given a move
	def move(self, direction):
		board = self.board
		position  = self.findZero()
		move = (position[0] + self.direction[direction][0], position[1] + self.direction[direction][1])
		#check for illegal moves
		if position[0] == 0 and direction == "up":
			return None
		if position[0] == 3 and direction == "down":
			return None
		if position[1] == 0 and direction == "left":
			return None
		if position[1] == 3 and direction == "right":
			return None
		
		board[position] = board[move]
		board[move] = 0
		return board

	#executes a move on the board
	def performMove(self, direction):
		if self.move(direction) == None:
			return False
		else:
			self.performMove(direction)
			return True
		
	#checks for sucessful board state
	def goalTest(self):
		count = 1
		for i in range(4):
			for j in range(4):
				if i == 3 and j == 3:
					return True
				elif self.board[i][j] != count:
					return False
				count += 1
	
	#Performs a series of m random moves where m is an inputted integer value
	def scramble(self, m):
		compass = ["up", "down", "left", "right"]
		for i in range(m):
			control = False
			while control == False:
				if self.performMove(compass[random.randint(0,3)]):
					control = True
					print(self.board)

#measures a manhattan heuristic	of a given board position			
def manhattan(game):
	h = 0
	for i in range(15):
		col = 0
		row = 0
		loc = np.where(game.board == i + 1)
		for j in range(i):
			col += 1
			if col ==4:
				col = 0
				row += 1
		h += abs(loc[0] - row) + abs(loc[1] - col)
	loc = np.where(game.board == 0)
	h += abs(3 - row) + abs(3 - col)
	return h

x = game()
print(x.board)
x.scramble(10)
print(manhattan(x))