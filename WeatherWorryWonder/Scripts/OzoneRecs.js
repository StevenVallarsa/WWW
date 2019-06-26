const theJson = {
    atHome: {
        title: 'At Home',
        tips: [
            'Reduce the amount of energy you use at home. Most of this area&rsquo;s electricity comes from coal-fired power plants that significantly contribute to ground-level ozone.',
            'Purchase ENERGY STAR equipment.',
            'Use compact fluorescent light bulbs. Turn off lights and appliances when they are not in use.',
            'Adjust the thermostat to a slightly higher setting in summer and consider installing a programmable thermostat.',
            'Avoid chemicals that contain volatile organic compounds (VOCs) such as spray paint, paint thinners, glue solvents, and pesticides.'
        ]
    },

    driving: {
        title: 'Driving',
        tips: [
            'Drive less by combining trips and planning in advance.',
            'Bike, walk or ride the bus when possible.',
            'Keep personal vehicles well-tuned and tires inflated properly. You can save up to 20% on the amount of gasoline you use.',
            'Pressure check vehicle gas caps annually and replace when necessary. A faulty gas cap can allow up to 30 gallons of fuel per year to evaporate.',
            'Refuel as late in the day as possible (after 7 pm preferably), especially on ozone alert days.',
            'Stop at the click. Don&rsquo;t top off your tank when you refuel. This keeps harmful fumes from being forced into the air.'
        ],
    },

    yardwork: {
        title: 'Doing Yardwork',
        tips: [
            'Mow as late as possible, preferably after 7 pm, when there is less sun and heat.',
            'Replace older gas cans with new &ldquo;no-spill&rdquo; gas cans for refueling equipment. Emissions from gasoline spills are major contributors to ozone and spilled gasoline costs you money.',
            'Practice low-maintenance lawn care, requiring less frequent mowing and less inputs of polluting chemical pesticides.',
            'Consider replacing any gasoline powered equipment with electric, batter or manual powered equipment.',
            'Convert lawn spaces to native plants to reduce the amount of mowing and watering.',
            'Avoid open burning.',
        ],
    },

    office: {
        title: 'At the Office',
        tips: [
            'Allow and promote teleconferencing instead of driving to meetings. If you must drive, carpool when possible.',
            'Bring your lunch, carpool or walk to lunch, especially on ozone alert days.',
            'Inquire about flexible work schedules that would promote driving less, such as the four day work week.',
            'Commute in style: bike, walk, carpool or take public transportation to work. Get in some exercise, good conversation or a little reading in the process!',
            'Purchase and use low-volatile organic compound (VOC) paints, solvents, pesticides, etc.',
            'Select printing companies that use soy-based inks or other low-emissions print processes.'
        ],
    },

    grilling: {
        title: 'Grilling',
        tips: [
            'Do not use lighter fluid. It pollutes on both evaporation and burning. Your food will taste better without it, too!',
            'Use a charcoal chimney instead of lighter fluid to start the coals. They are easy to use and leave no telltale taste in the food.',
            'Choose briquettes that are additive-free and avoid any added chemicals flavors to the food.',
            'Gas grills emit less pollution than charcoal grills.',
            'Postpone grilling until evening on ozone alert days.'
        ],
    }
};

const theSwitch = arg => {
    $('#jumbotron').attr('class', arg);
    $('#jumbotron h1').text(theJson[arg]['title']);

    let tips = '';
    for (let i = 0; i < theJson[arg]['tips'].length; i++) {
        tips += `<li>${theJson[arg]['tips'][i]}</li>`;
    }
    $('.tips').html(tips);

    $('.nav-item').removeClass('active');
    $('nav #' + arg).addClass('active');
}

$(() => {
    theSwitch('atHome');

    $('.nav-item').on('click', function () {
        theSwitch($(this).attr('id'));
        $('html,body').scrollTop(0);
    });

    console.log('Yes, we are aware that jQuery is dying.');
})
