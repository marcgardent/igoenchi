import numpy;

import cntk;
import cntk.models;
import cntk.layers;
import cntk.ops;
import cntk.io;
import cntk.utils.progress_print;
import cntk.persist;
import os;

class Context:

    def __init__(self):
        self.BOARD_LENGTH = 19
        self.BOARD_DIM = self.BOARD_LENGTH *self.BOARD_LENGTH;
        self.NB_EPOCH = 5
        self.NB_ITER = 5 #number of reinforcement learning iterations
        self.SHAPE_INPUT = ( self.BOARD_LENGTH, self.BOARD_LENGTH) #self.BOARD_DIM 
        self.SHAPE_OUTPUT = ( self.BOARD_LENGTH, self.BOARD_LENGTH) #self.BOARD_DIM 

def create_reader(path, is_training, context):
    return cntk.io.MinibatchSource(cntk.io.CTFDeserializer(path, cntk.io.StreamDefs(
        features  = cntk.io.StreamDef(field='g', shape=context.BOARD_DIM, is_sparse=False),
        labels    = cntk.io.StreamDef(field='n',   shape=context.BOARD_DIM, is_sparse=False)
    )), randomize=is_training, epoch_size =  cntk.io.MinibatchSource.infinitely_repeat if is_training else cntk.io.MinibatchSource.full_data_sweep)

 
# Train and evaluate the network.
def train_and_evaluate(context, reader_train, reader_test, max_epochs):
     

    # Input variables denoting the features and label data
    input_var = cntk.ops.input_variable(context.SHAPE_INPUT, numpy.float32, is_sparse=False, name="g") # eg img (num_channels, image_height, image_width)
    label_var = cntk.ops.input_variable(context.SHAPE_OUTPUT, numpy.float32, is_sparse=False, name="n")

    # create model, and configure learning parameters 
    z = create_model(context, input_var, label_var)
    lr_per_mb = [1.0]*80+[0.1]*40+[0.01] # ????

    print ("Parameters : ",len(z.parameters))
              
    # loss and metric
    ce = cntk.ops.cross_entropy_with_softmax(z, label_var)
    pe = cntk.ops.classification_error(z, label_var)

    # shared training parameters 
    epoch_size = 50000  # for now we manually specify epoch size
    minibatch_size = 128
    momentum_time_constant = -minibatch_size/numpy.log(0.9)
    l2_reg_weight = 0.0001

    # Set learning parameters
    lr_per_sample = [lr/minibatch_size for lr in lr_per_mb]
    lr_schedule = cntk.learner.learning_rate_schedule(lr_per_sample, epoch_size=epoch_size, unit=cntk.learner.UnitType.sample)
    mm_schedule = cntk.learner.momentum_as_time_constant_schedule(momentum_time_constant)
    


    # trainer object
    learner = cntk.learner.momentum_sgd(
        parameters=z.parameters,
        lr=lr_schedule,
        momentum=mm_schedule,
        l2_regularization_weight=l2_reg_weight
    )

    trainer = cntk.Trainer(
        model=z,
        loss_function=ce,
        eval_function=pe,
        parameter_learners=learner
    )

    # define mapping from reader streams to network inputs
    input_map = {
        input_var: reader_train.streams.features,
        label_var: reader_train.streams.labels
    }
    
    cntk.utils.progress_print.log_number_of_parameters(z) ; print()
    progress_printer = cntk.utils.progress_print.ProgressPrinter(tag='Training')

    # perform model training
    for epoch in range(max_epochs):       # loop over epochs
        sample_count = 0
        while sample_count < epoch_size:  # loop over minibatches in the epoch
            data = reader_train.next_minibatch(min(minibatch_size, epoch_size-sample_count), input_map=input_map) # fetch minibatch.
            trainer.train_minibatch(data)                                   # update model with it
            sample_count += trainer.previous_minibatch_sample_count         # count samples processed so far
            progress_printer.update_with_trainer(trainer, with_metric=True) # log progress
        progress_printer.epoch_summary(with_metric=True)
        cntk.persist.save_model(z, os.path.join("D:\\go-dataset\\dnn", "gotit_{}.dnn".format(epoch)))
    
    # Evaluation parameters
    epoch_size     = 10000
    minibatch_size = 16

    # process minibatches and evaluate the model
    metric_numer    = 0
    metric_denom    = 0
    sample_count    = 0
    minibatch_index = 0

    while sample_count < epoch_size:
        current_minibatch = min(minibatch_size, epoch_size - sample_count)
        # Fetch next test min batch.
        data = reader_test.next_minibatch(current_minibatch, input_map=input_map)
        # minibatch data to be trained with
        metric_numer += trainer.test_minibatch(data) * current_minibatch
        metric_denom += current_minibatch
        # Keep track of the number of samples processed so far.
        sample_count += data[label_var].num_samples
        minibatch_index += 1

    print("")
    print("Final Results: Minibatch[1-{}]: errs = {:0.2f}% * {}".format(minibatch_index+1, (metric_numer*100.0)/metric_denom, metric_denom))
    print("")

    return metric_numer/metric_denom


def create_model(ctx,input_var, ouput_var):

    """    
    #keras
    model = Sequential()
    model.add(Dense(2 * INPUT_DIM, input_dim=INPUT_DIM, activation='relu'))
    model.add(Dropout(0.2))
    model.add(Dense(2 * INPUT_DIM, activation='tanh'))
    model.add(Dropout(0.2))
    model.add(Dense(OUTPUT_DIM))
    model.add(Activation('softmax'))
    model.compile(optimizer='adam', loss='sparse_categorical_crossentropy', metrics=['accuracy'])
    """
    #ctx.SHAPE_INPUT
    model = cntk.models.Sequential ([
        cntk.layers.Dense(ctx.SHAPE_INPUT, activation=cntk.ops.relu),
        cntk.layers.Dropout(0.2),
        cntk.layers.Dense(ctx.SHAPE_INPUT, activation=cntk.ops.tanh),
        cntk.layers.Dropout(0.2),
        cntk.layers.Dense(ctx.SHAPE_OUTPUT, activation=cntk.ops.softmax)
    ])
    
    return model(input_var);


  
if __name__ == '__main__':

    cntk.set_default_device(cntk.gpu(0))
    
    ctx = Context(); 
    print("loading train dataset...");
    reader_train =create_reader("D:\\go-dataset\\kgs-19-2011.small.ctf", is_training=True, context=ctx);
    print("loading test dataset...");
    reader_test =create_reader("D:\\go-dataset\\kgs-19-2009.small.ctf", is_training=True, context=ctx);

    train_and_evaluate(ctx, reader_train, reader_test, 1000);
